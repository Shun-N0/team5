using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerControllerStage00 : MonoBehaviour
{
    private static Sprite stage00BulletSprite;

    [Header("移動設定")]
    public float speed = 8f;
    public float xLimit = 2.1f; 
    public float yMin = -4.5f;
    public float yMax = 4.5f;

    [Header("発射設定")]
    public GameObject bulletPrefab;
    public float shotInterval = 0.15f;
    public int attackPower = 1;

    [Header("Stage00 弾数設定")]
    public int bulletCount = 1;
    public float bulletSpacingX = 0.3f;
    public int maxBulletsPerRow = 10;
    public int bulletsPerPowerLevel = 15;

    private float timer = 0f;

    [Header("残機設定")]
    public int lives = 1;
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

    [Header("音量調整")]
    [Range(0, 1)] public float shotVolume = 0.3f;
    [Range(0, 1)] public float damageVolume = 0.8f;
    [Range(0, 1)] public float explosionVolume = 1.0f;

    [Header("ゲームオーバー演出")]
    public float gameOverDelay = 1.5f; 

    // ★追加：進化設定
    [Header("進化設定")]
    public Sprite evolvedSprite;    // 進化後の戦闘機画像
    private bool isEvolved = false; // 進化しているか

    private bool isStunned = false;

    void Start()
    {
    startPosition = transform.position;
    spriteRenderer = GetComponent<SpriteRenderer>();
    audioSource = GetComponent<AudioSource>();
    mainCamera = Camera.main;

    Debug.Log("初期弾数 = " + bulletCount);

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
        targetPosition.y = yMin;
        targetPosition.z = transform.position.z;
        transform.position = targetPosition;
    }

    void Shoot() 
    { 
        if (bulletPrefab != null) 
        {
            int safeBulletCount = Mathf.Max(1, bulletCount);
            int bulletsPerRow = CalculateBulletsPerRow();
            int visibleBulletCount = Mathf.Min(safeBulletCount, bulletsPerRow);
            int powerLevel = Mathf.Max(0, (safeBulletCount - 1) / Mathf.Max(1, bulletsPerPowerLevel));
            Color bulletColor = GetBulletColor(powerLevel);

            for (int i = 0; i < visibleBulletCount; i++)
            {
                float rowWidth = (visibleBulletCount - 1) * bulletSpacingX;
                float xOffset = i * bulletSpacingX - rowWidth * 0.5f;
                float yOffset = 0.1f;

                GameObject bulletObject = Instantiate(
                    bulletPrefab,
                    transform.position + new Vector3(xOffset, yOffset, 0),
                    Quaternion.identity
                );

                Bullet bullet = bulletObject.GetComponent<Bullet>();
                if (bullet != null) bullet.attackPower = attackPower + powerLevel;

                SpriteRenderer bulletRenderer = bulletObject.GetComponent<SpriteRenderer>();
                if (bulletRenderer != null)
                {
                    bulletRenderer.sprite = GetStage00BulletSprite();
                    bulletRenderer.color = bulletColor;
                }
            }

            if (shotSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shotSound, shotVolume);
            }
        }
    }

    private int CalculateBulletsPerRow()
    {
        float playableWidth = Mathf.Max(0.1f, xLimit * 2f);
        int fitCount = Mathf.Max(1, Mathf.FloorToInt(playableWidth / bulletSpacingX) + 1);
        return Mathf.Max(1, Mathf.Min(maxBulletsPerRow, fitCount));
    }

    private Color GetBulletColor(int powerLevel)
    {
        Color[] colors =
        {
            Color.white,
            new Color(1f, 0.95f, 0f),
            new Color(0f, 0.9f, 0.18f),
            new Color(0f, 0.35f, 1f),
            new Color(0.65f, 0f, 1f),
            new Color(1f, 0f, 0.08f),
            new Color(1f, 0.63f, 0f)
        };

        return colors[Mathf.Min(powerLevel, colors.Length - 1)];
    }

    private static Sprite GetStage00BulletSprite()
    {
        if (stage00BulletSprite != null) return stage00BulletSprite;

        const int width = 24;
        const int height = 40;
        Texture2D texture = new Texture2D(width, height);
        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedX = (x - center.x) / (width * 0.38f);
                float normalizedY = (y - center.y) / (height * 0.48f);
                float distance = normalizedX * normalizedX + normalizedY * normalizedY;
                float alpha = Mathf.Clamp01(1f - (distance - 0.45f) / 0.55f);
                Color color = Color.white;
                color.a = alpha;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        stage00BulletSprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, 22f);
        return stage00BulletSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ★追加：進化アイテム(Itemタグ)を拾った時の処理
        if (collision.gameObject.CompareTag("Item"))
        {
            isEvolved = true;
            if (evolvedSprite != null) spriteRenderer.sprite = evolvedSprite; // 見た目を変える
            Destroy(collision.gameObject); // アイテムを消す
            return; // ダメージ処理をスキップ
        }

        if (collision.GetComponent<EarthBullet>() != null)
        {
            Destroy(collision.gameObject);
            PlayerDamaged();
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
            if (lifeIcons[i] != null)
            {
                if (i < lives) lifeIcons[i].SetActive(true);
                else lifeIcons[i].SetActive(false);
            }
        }
    }

        // 弾数を増やす
    public void AddBullets(int amount)
    {
        bulletCount = Mathf.Max(1, bulletCount + amount);
    }

    // 弾数を減らす（最低1発）
    public void RemoveBullets(int amount)
    {
        bulletCount = Mathf.Max(1, bulletCount - amount);
    }

    // 弾数を2倍
    public void MultiplyBullets(int value)
    {
        bulletCount = Mathf.Max(1, bulletCount * value);
    }

    // 弾数を半分（切り上げ）
    public void DivideBullets(int value)
    {
        bulletCount = Mathf.Max(1, Mathf.CeilToInt((float)bulletCount / value));
    }
}
