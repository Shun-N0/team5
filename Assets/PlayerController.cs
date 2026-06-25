using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public int lives = 3;
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

    // ★修正：進化状態によって弾の数を変える
    void Shoot() 
    { 
        if (bulletPrefab != null) 
        {
            if (isEvolved)
            {
                // 進化時：左右の翼から2発出す（位置 0.25f は適宜調整してください）
                GameObject b1 = Instantiate(bulletPrefab, transform.position + new Vector3(0.25f, 0.1f, 0), Quaternion.identity);
                GameObject b2 = Instantiate(bulletPrefab, transform.position + new Vector3(-0.25f, 0.1f, 0), Quaternion.identity);
                b1.GetComponent<Bullet>().attackPower = attackPower;
                b2.GetComponent<Bullet>().attackPower = attackPower;
            }
            else
            {
                // 通常時：真ん中から1発
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
}