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
    public int lives = 5; // ステージ2に合わせて5に設定（任意で調整可）
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
    public Sprite evolvedSprite;    // 進化後の戦闘機画像
    private bool isEvolved = false; // 進化しているか

    [Header("バリア設定")]
    public GameObject shieldObject; // バリアの見た目（子オブジェクト）
    // バリアで防げるタグのリスト
    public List<string> protectTags = new List<string> { "Enemy", "EnemyBullet", "StunBullet", "EarthBullet" }; 
    private bool hasShield = false;

    // スタン（停止）状態の管理
    private bool isStunned = false;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;

        // 始まった時はバリアをオフにしておく
        if (shieldObject != null) shieldObject.SetActive(false);
        
        UpdateLifeUI(); 
    }

    void Update()
    {
        // 死亡演出中、またはスタン中は操作を無効化
        if (!spriteRenderer.enabled && lives <= 0) return;
        if (isStunned) return;

        MoveToMousePosition();

        timer += Time.deltaTime;
        // マウス左クリック（指タッチ）で連射
        if (Input.GetMouseButton(0) && timer >= shotInterval)
        {
            Shoot();
            timer = 0f;
        }

        // 無敵時間の点滅処理
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
        if (bulletPrefab != null) 
        {
            if (isEvolved)
            {
                // 進化時：左右から2連射
                GameObject b1 = Instantiate(bulletPrefab, transform.position + new Vector3(0.25f, 0.1f, 0), Quaternion.identity);
                GameObject b2 = Instantiate(bulletPrefab, transform.position + new Vector3(-0.25f, 0.1f, 0), Quaternion.identity);
                b1.GetComponent<Bullet>().attackPower = attackPower;
                b2.GetComponent<Bullet>().attackPower = attackPower;
            }
            else
            {
                // 通常時
                GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bulletObject.GetComponent<Bullet>().attackPower = attackPower;
            }

            if (shotSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shotSound, shotVolume);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // --- 1. アイテム取得判定 (最優先) ---

        // 進化アイテム
        if (collision.gameObject.CompareTag("Item"))
        {
            isEvolved = true;
            if (evolvedSprite != null) spriteRenderer.sprite = evolvedSprite;
            Destroy(collision.gameObject);
            return;
        }

        // バリアアイテム
        if (collision.gameObject.CompareTag("ShieldItem"))
        {
            ActivateShield();
            Destroy(collision.gameObject);
            return;
        }

        // --- 2. バリアでの防御判定 ---
        if (hasShield)
        {
            // 当たった相手のタグが「防げるリスト」に入っているか、またはEarthBulletスクリプトを持っているか
            if (protectTags.Contains(collision.gameObject.tag) || collision.GetComponent<EarthBullet>() != null)
            {
                hasShield = false;
                if (shieldObject != null) shieldObject.SetActive(false);
                
                Destroy(collision.gameObject); // 敵や弾を消す
                Debug.Log("バリアで防ぎました！");
                return; // ここで終了。ダメージは受けない
            }
        }

        // --- 3. ここから下は「バリアがない時」の通常ダメージ判定 ---

        // 地球の弾（即死/ゲームオーバー）
        if (collision.GetComponent<EarthBullet>() != null)
        {
            Destroy(collision.gameObject);
            StageManager.Instance?.TriggerGameOver();
            return;
        }

        // スタン弾
        if (collision.gameObject.CompareTag("StunBullet"))
        {
            if (isInvincible) return;
            Destroy(collision.gameObject);
            GetStunned(2.0f);
            return;
        }

        // 敵本体、または敵の弾
        if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("Enemy"))
        {
            if (isInvincible) return;

            if (collision.gameObject.CompareTag("EnemyBullet"))
            {
                Destroy(collision.gameObject);
            }
            PlayerDamaged();
        }
    }

    public void ActivateShield()
    {
        hasShield = true;
        if (shieldObject != null) shieldObject.SetActive(true);
        Debug.Log("バリア展開！");
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
            if (damageSound != null && audioSource != null)
                audioSource.PlayOneShot(damageSound, damageVolume);

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
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume);

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
}