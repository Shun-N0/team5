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
    public int attackPower = 1;
    private float timer = 0f;

    // 他のステージ(Stage00)との互換性用
    [HideInInspector] public float shotInterval = 0.15f; 

    [Header("レベル別 連射速度(間隔)設定")]
    public float level1ShotInterval = 0.18f;
    public float level2ShotInterval = 0.2f;
    public float level3ShotInterval = 0.23f;

    [Header("レベル別 弾速設定")]
    public float level1BulletSpeed = 10f; 
    public float level2BulletSpeed = 12f; 
    public float level3BulletSpeed = 15f; 

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
    public int evolveLevel = 1;      
    public Sprite level2Sprite;      
    public Sprite level3Sprite;      
    
    [Header("進化後の弾の角度・幅")]
    public float level2Angle = 0f;      
    public float level2Offset = 0.2f;   
    public float level3Angle = 20f;     
    public float level3Offset = 0.3f;   

    [Header("バリア設定")]
    public GameObject shieldObject; 
    public List<string> protectTags = new List<string> { "Enemy", "EnemyBullet", "StunBullet", "EarthBullet" }; 
    private bool hasShield = false;

    private bool isStunned = false;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

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

        shotInterval = GetCurrentShotInterval();

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

    float GetCurrentShotInterval()
    {
        if (evolveLevel == 1) return level1ShotInterval;
        if (evolveLevel == 2) return level2ShotInterval;
        return level3ShotInterval;
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

        if (evolveLevel == 1) CreateBullet(transform.position, 0f, level1BulletSpeed);
        else if (evolveLevel == 2) {
            CreateBullet(transform.position + new Vector3(level2Offset, 0.1f, 0), -level2Angle, level2BulletSpeed);
            CreateBullet(transform.position + new Vector3(-level2Offset, 0.1f, 0), level2Angle, level2BulletSpeed);
        }
        else if (evolveLevel >= 3) {
            CreateBullet(transform.position, 0f, level3BulletSpeed); 
            CreateBullet(transform.position + new Vector3(level3Offset, 0.1f, 0), -level3Angle, level3BulletSpeed); 
            CreateBullet(transform.position + new Vector3(-level3Offset, 0.1f, 0), level3Angle, level3BulletSpeed);  
        }

        if (shotSound != null && audioSource != null)
            audioSource.PlayOneShot(shotSound, shotVolume);
    }

    void CreateBullet(Vector3 pos, float angle, float bSpeed)
    {
        GameObject b = Instantiate(bulletPrefab, pos, Quaternion.Euler(0, 0, angle));
        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null) {
            bulletScript.attackPower = attackPower;
            bulletScript.speed = bSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            evolveLevel++;
            if (evolveLevel > 3) evolveLevel = 3;
            if (evolveLevel == 2 && level2Sprite != null) spriteRenderer.sprite = level2Sprite;
            if (evolveLevel == 3 && level3Sprite != null) spriteRenderer.sprite = level3Sprite;
            if (GetComponent<PolygonCollider2D>() != null) {
                Destroy(GetComponent<PolygonCollider2D>());
                gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
            }
            Destroy(collision.gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("ShieldItem")) {
            if (hasShield) { Destroy(collision.gameObject); return; }
            ActivateShield();
            Destroy(collision.gameObject);
            return;
        }

        if (hasShield) {
            if (IsProtectedByShield(collision)) {
                hasShield = false;
                if (shieldObject != null) shieldObject.SetActive(false);
                if (!IsBossCollision(collision)) Destroy(collision.gameObject);
                return;
            }
        }

        if (collision.GetComponent<EarthBullet>() != null) { Destroy(collision.gameObject); if (StageManager.Instance != null) StageManager.Instance.TriggerGameOver(); return; }
        if (collision.gameObject.CompareTag("StunBullet")) { if (isInvincible) return; Destroy(collision.gameObject); GetStunned(2.0f); return; }
        if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("Enemy")) {
            if (isInvincible) return;
            if (collision.gameObject.CompareTag("EnemyBullet")) Destroy(collision.gameObject);
            PlayerDamaged();
        }
    }

    private bool IsProtectedByShield(Collider2D c) { return protectTags.Contains(c.gameObject.tag) || c.GetComponent<EarthBullet>() != null; }
    private bool IsBossCollision(Collider2D c) { return c.GetComponent<Boss2>() != null || c.GetComponent<Boss3>() != null || c.GetComponent<MechaJellyfishBoss>() != null; }

    public void ActivateShield() { EnsureShieldObject(); hasShield = true; if (shieldObject != null) shieldObject.SetActive(true); }
    private void EnsureShieldObject() { if (shieldObject == null) CreateDefaultShieldObject(); AdjustShieldScale(); }
    private void CreateDefaultShieldObject() { GameObject ds = new GameObject("Shield"); ds.transform.SetParent(transform, false); SpriteRenderer sr = ds.AddComponent<SpriteRenderer>(); sr.color = new Color(0.2f, 0.9f, 1f, 0.55f); shieldObject = ds; }
    
    private void AdjustShieldScale() {
        if (shieldObject == null) return;
        shieldObject.transform.localPosition = Vector3.zero;
        shieldObject.transform.localScale = Vector3.one * 3.0f; 
    }

    public void GetStunned(float d) { if (!isStunned && gameObject.activeInHierarchy) StartCoroutine(StunRoutine(d)); }
    IEnumerator StunRoutine(float d) { isStunned = true; Color oc = spriteRenderer.color; spriteRenderer.color = Color.cyan; yield return new WaitForSeconds(d); spriteRenderer.color = oc; isStunned = false; }
    
    void PlayerDamaged() { 
        lives--; UpdateLifeUI(); 
        if (lives > 0) { 
            if (damageSound != null && audioSource != null) audioSource.PlayOneShot(damageSound, damageVolume); 
            isInvincible = true; invincibleTimer = invincibleDuration; blinkTimer = blinkInterval; 
        } else { StartCoroutine(GameOverRoutine()); } 
    }

    IEnumerator GameOverRoutine() { 
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, explosionVolume); 
        spriteRenderer.enabled = false; isInvincible = true; 
        yield return new WaitForSeconds(gameOverDelay); 
        if (StageManager.Instance != null) StageManager.Instance.TriggerGameOver(); 
    }

    void UpdateLifeUI() { if (lifeIcons == null) return; for (int i = 0; i < lifeIcons.Length; i++) { if (lifeIcons[i] != null) lifeIcons[i].SetActive(i < lives); } }
}