using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 8f;
    public float xLimit = 2.1f; 
    public float yMin = -4.5f;
    public float yMax = 4.5f;

    [Header("発射設定")]
    public GameObject bulletPrefab;
    public float shotInterval = 0.15f;
    public int attackPower = 1;
    private float timer = 0f;

    [Header("残機設定")]
    public int lives = 5; 
    public GameObject[] lifeIcons; 
    private Vector3 startPosition;

    [Header("無敵設定")]
    public float invincibleDuration = 2f;
    public float blinkInterval = 0.1f;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    private float blinkTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    [Header("サウンド設定")]
    public AudioClip shotSound;
    public AudioClip damageSound;
    public AudioClip explosionSound;
    private AudioSource audioSource;

    [Header("音量調整 (0.0〜1.0)")]
    [Range(0, 1)] public float shotVolume = 0.3f;
    [Range(0, 1)] public float damageVolume = 0.8f;
    [Range(0, 1)] public float explosionVolume = 1.0f;

    [Header("ゲームオーバー演出")]
    public float gameOverDelay = 1.5f; 

    [Header("進化設定")]
    public int evolveLevel = 1;      // 現在のレベル (1〜3)
    public Sprite level2Sprite;      // LEVEL 2 の画像
    public Sprite level3Sprite;      // LEVEL 3 の画像
    
    [Header("LEVEL 2 弾の設定 (2連射)")]
    public float level2Angle = 0f;      // 弾の広がり角度
    public float level2Offset = 0.2f;   // 左右の弾の間隔

    [Header("LEVEL 3 弾の設定 (3連射)")]
    public float level3Angle = 20f;     // 左右の弾の角度
    public float level3Offset = 0.3f;   // 左右の弾の間隔

    [Header("バリア設定")]
    public GameObject shieldObject; // バリアの見た目（子オブジェクト）
    [SerializeField] private float shieldWorldSize = 1.2f;
    public List<string> protectTags = new List<string> { "Enemy", "EnemyBullet", "StunBullet", "EarthBullet" }; 
    private bool hasShield = false;

    private bool isStunned = false;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;

        EnsureShieldObject();
        if (shieldObject != null) shieldObject.SetActive(false);
        UpdateLifeUI(); 
    }

    void Update()
    {
        if (!spriteRenderer.enabled && lives <= 0) return;
        if (isStunned) return;

        MoveToMousePosition();

        timer += Time.deltaTime;
        if (Input.GetMouseButton(0) && timer >= shotInterval)
        {
            Shoot();
            timer = 0f;
        }

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                blinkTimer = blinkInterval;
                if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null) spriteRenderer.enabled = true;
            }
        }
    }

    private void MoveToMousePosition()
    {
        if (mainCamera == null) return;
        Vector3 mousePosition = Input.mousePosition;
        if (mousePosition.x < 0f || mousePosition.x > Screen.width || mousePosition.y < 0f || mousePosition.y > Screen.height) return;

        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        targetPosition.x = Mathf.Clamp(targetPosition.x, -xLimit, xLimit);
        targetPosition.y = Mathf.Clamp(targetPosition.y, yMin, yMax);
        targetPosition.z = transform.position.z;
        transform.position = targetPosition;
    }

    void Shoot() 
    { 
        if (bulletPrefab == null) return;

        if (evolveLevel == 1)
        {
            // LEVEL 1: 中央から1発
            CreateBullet(transform.position, 0f);
        }
        else if (evolveLevel == 2)
        {
            // LEVEL 2: 2連射（専用の角度と幅を使用）
            CreateBullet(transform.position + new Vector3(level2Offset, 0.1f, 0), -level2Angle);
            CreateBullet(transform.position + new Vector3(-level2Offset, 0.1f, 0), level2Angle);
        }
        else if (evolveLevel >= 3)
        {
            // LEVEL 3: 3連射（専用の角度と幅を使用）
            CreateBullet(transform.position, 0f); // 中央
            CreateBullet(transform.position + new Vector3(level3Offset, 0.1f, 0), -level3Angle); // 右斜め
            CreateBullet(transform.position + new Vector3(-level3Offset, 0.1f, 0), level3Angle);  // 左斜め
        }

        if (shotSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shotSound, shotVolume);
        }
    }

    void CreateBullet(Vector3 pos, float angle)
    {
        GameObject b = Instantiate(bulletPrefab, pos, Quaternion.Euler(0, 0, angle));
        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null) bulletScript.attackPower = attackPower;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // --- 1. アイテム取得判定 ---
        if (collision.gameObject.CompareTag("Item"))
        {
            evolveLevel++;
            if (evolveLevel > 3) evolveLevel = 3;

            if (evolveLevel == 2 && level2Sprite != null) spriteRenderer.sprite = level2Sprite;
            if (evolveLevel == 3 && level3Sprite != null) spriteRenderer.sprite = level3Sprite;

            if (GetComponent<PolygonCollider2D>() != null)
            {
                Destroy(GetComponent<PolygonCollider2D>());
                gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
            }

            Destroy(collision.gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("ShieldItem"))
        {
            if (hasShield)
            {
                Destroy(collision.gameObject);
                Debug.Log("既にバリアがあるため、バリアアイテムは効果なし");
                return;
            }

            ActivateShield();
            Destroy(collision.gameObject);
            return;
        }

        // --- 2. バリア防御判定（ダメージより先に判定） ---
        if (hasShield)
        {
            if (IsProtectedByShield(collision))
            {
                if (IsBossCollision(collision))
                {
                    // ボスに当たった場合は、バリアだけ消して、ボス（相手）は消さない
                    DeactivateShield();
                    Debug.Log("ボスに接触！バリアで身を守りました（ボスは消えません）");
                }
                else
                {
                    DeactivateShield();
                    Destroy(collision.gameObject); 
                    Debug.Log("バリアで防御成功！");
                }
                
                return; // 本体へのダメージは受けない
            }
        }
        // --- 3. ダメージ判定 ---
        if (collision.GetComponent<EarthBullet>() != null)
        {
            Destroy(collision.gameObject);
            StageManager.Instance?.TriggerGameOver();
            return;
        }

        if (collision.gameObject.CompareTag("StunBullet"))
        {
            if (isInvincible) return;
            Destroy(collision.gameObject);
            GetStunned(2.0f);
            return;
        }

        if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("Enemy"))
        {
            if (isInvincible) return;
            if (collision.gameObject.CompareTag("EnemyBullet")) Destroy(collision.gameObject);
            PlayerDamaged();
        }
    }

    public void ActivateShield()
    {
        EnsureShieldObject();
        hasShield = true;
        if (shieldObject != null) shieldObject.SetActive(true);
    }

    private void DeactivateShield()
    {
        hasShield = false;
        if (shieldObject != null) shieldObject.SetActive(false);
    }

    private bool IsProtectedByShield(Collider2D collision)
    {
        return protectTags.Contains(collision.gameObject.tag) || collision.GetComponent<EarthBullet>() != null;
    }

    private bool IsBossCollision(Collider2D collision)
    {
        return collision.GetComponent<Boss2>() != null
            || collision.GetComponent<Boss3>() != null
            || collision.GetComponent<MechaJellyfishBoss>() != null;
    }

    private void EnsureShieldObject()
    {
        if (shieldObject == null)
        {
            CreateDefaultShieldObject();
            return;
        }

        if (shieldObject.GetComponentInChildren<SpriteRenderer>(true) == null)
        {
            SpriteRenderer shieldRenderer = shieldObject.AddComponent<SpriteRenderer>();
            shieldRenderer.sprite = CreateShieldSprite();
            shieldRenderer.color = new Color(0.2f, 0.9f, 1f, 0.55f);
            shieldRenderer.sortingOrder = 100;
        }

        AdjustShieldScale();
    }

    private void CreateDefaultShieldObject()
    {
        GameObject defaultShield = new GameObject("Shield");
        defaultShield.transform.SetParent(transform, false);

        SpriteRenderer shieldRenderer = defaultShield.AddComponent<SpriteRenderer>();
        shieldRenderer.sprite = CreateShieldSprite();
        shieldRenderer.color = new Color(0.2f, 0.9f, 1f, 0.55f);
        shieldRenderer.sortingOrder = 100;

        shieldObject = defaultShield;
        AdjustShieldScale();
    }

    private void AdjustShieldScale()
    {
        if (shieldObject == null) return;

        float parentScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        if (parentScale <= 0f) parentScale = 1f;

        float spriteSize = 1f;
        SpriteRenderer shieldRenderer = shieldObject.GetComponentInChildren<SpriteRenderer>(true);
        if (shieldRenderer != null && shieldRenderer.sprite != null)
        {
            Vector2 spriteBoundsSize = shieldRenderer.sprite.bounds.size;
            spriteSize = Mathf.Max(spriteBoundsSize.x, spriteBoundsSize.y);
        }

        if (spriteSize <= 0f) spriteSize = 1f;

        float localSize = shieldWorldSize / (parentScale * spriteSize);
        shieldObject.transform.localPosition = Vector3.zero;
        shieldObject.transform.localScale = Vector3.one * localSize;
    }

    private Sprite CreateShieldSprite()
    {
        const int textureSize = 96;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = Vector2.one * (textureSize - 1) / 2f;
        float outerRadius = textureSize * 0.46f;
        float innerRadius = textureSize * 0.37f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                bool isRing = distance <= outerRadius && distance >= innerRadius;
                bool isFill = distance < innerRadius;

                if (isRing)
                    texture.SetPixel(x, y, Color.white);
                else if (isFill)
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, 0.25f));
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, textureSize);
    }

    public void GetStunned(float duration)
    {
        if (!isStunned && gameObject.activeInHierarchy) StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.cyan; 
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
        isStunned = false;
    }

    void PlayerDamaged()
    {
        lives--;
        UpdateLifeUI(); 
        if (lives > 0)
        {
            if (damageSound != null && audioSource != null) audioSource.PlayOneShot(damageSound, damageVolume);
            isInvincible = true;
            invincibleTimer = invincibleDuration;
            blinkTimer = blinkInterval;
        }
        else
        {
            StartCoroutine(GameOverRoutine());
        }
    }

    IEnumerator GameOverRoutine()
    {
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume);
        spriteRenderer.enabled = false;
        isInvincible = true; 
        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene("GameOverScene");
    }

    void UpdateLifeUI()
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null) lifeIcons[i].SetActive(i < lives);
        }
    }
}
