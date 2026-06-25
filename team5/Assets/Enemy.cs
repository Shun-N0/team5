using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2f;
    public float descentSpeed = 0.5f;
    public float minDescentSpeed = 0.8f;
    public float maxDescentSpeed = 1.8f;
    private int direction = 1;

    [Header("攻撃設定")]
    public GameObject enemyBulletPrefab;
    public float shotInterval = 3f;
    private float timer;

    [Header("弾の設定")]
    public float minShotInterval = 1.0f;
    public float maxShotInterval = 3.0f;
    private float currentShotInterval;
    // 弾速：全敵で共通の固定値（同じ種類の敵なら同じスピード）
    public float bulletSpeed = 3f;
    private float currentBulletSpeed;

    [Header("弾の大きさ設定")]
    // 弾サイズは敵サイズに対する比率で決定（大きい敵→大きい弾、小さい敵→小さい弾）
    public float bulletScaleRatio = 5f;
    private float currentBulletScale;

    [Header("出現・移動範囲")]
    public float spawnYOffset = 1.0f;  
    public float minMoveXRange = 1.0f;
    public float maxMoveXRange = 2.3f;
    private float currentMoveXRange;

    [Header("敵本体のサイズ")]
    public float minScale = 0.5f;
    public float maxScale = 1.5f;

    [Header("体力・スコア設定")]
    public int maxHealth = 1;
    public int scoreValue = 100;
    private int currentHealth;

    [Header("サウンド設定")]
    public AudioClip killSound;
    [Range(0, 1)] public float killVolume = 1.0f;

    // ★追加：エフェクト設定
    [Header("エフェクト設定")]
    public GameObject explosionPrefab; // 爆発パーティクルのプレハブ

    void Start()
    {
        currentHealth = maxHealth;
        InitializeEnemy();
        timer = Random.Range(0f, currentShotInterval);
    }

    void Update()
    {
        // 1. 移動処理
        float xMove = direction * speed * Time.deltaTime;
        float yMove = -descentSpeed * Time.deltaTime;
        transform.Translate(new Vector3(xMove, yMove, 0));

        if (transform.position.x > currentMoveXRange)
        {
            direction = -1;
            transform.position = new Vector3(currentMoveXRange, transform.position.y, 0);
        }
        else if (transform.position.x < -currentMoveXRange)
        {
            direction = 1;
            transform.position = new Vector3(-currentMoveXRange, transform.position.y, 0);
        }

        if (transform.position.y < -5.5f) { Destroy(gameObject); }

        // 2. 射撃処理
        if (IsOnScreen())
        {
            timer += Time.deltaTime;
            if (timer > currentShotInterval)
            {
                Shoot();
                timer = 0;
                currentShotInterval = Random.Range(minShotInterval, maxShotInterval);
            }
        }
    }

    bool IsOnScreen()
    {
        float cameraTop = Camera.main.transform.position.y + Camera.main.orthographicSize;
        return transform.position.y <= cameraTop;
    }

    void Shoot()
    {
        if (enemyBulletPrefab != null)
        {
            GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.localScale = new Vector3(currentBulletScale, currentBulletScale, 1);
            bullet.SendMessage("SetSpeed", currentBulletSpeed, SendMessageOptions.DontRequireReceiver);
        }
    }

    void InitializeEnemy()
    {
        float cameraTop = Camera.main.transform.position.y + Camera.main.orthographicSize + spawnYOffset;
        float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;

        currentMoveXRange = Random.Range(minMoveXRange, maxMoveXRange);
        float randomX = Random.Range(-cameraHalfWidth, cameraHalfWidth);
        float randomY = cameraTop;
        transform.position = new Vector3(randomX, randomY, 0);

        float randomSize = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomSize, randomSize, 1);

        // 弾速は全敵で同じ固定値
        currentBulletSpeed = bulletSpeed;
        // 弾サイズは敵自身のサイズに比例（randomSize × 比率）
        currentBulletScale = randomSize * bulletScaleRatio;
        currentShotInterval = Random.Range(minShotInterval, maxShotInterval);
        
        speed = Random.Range(1f, 4f);
        descentSpeed = Random.Range(minDescentSpeed, maxDescentSpeed);
        timer = 0;
        direction = (Random.value > 0.5f) ? 1 : -1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            int damage = bullet != null ? bullet.attackPower : 1;

            Destroy(collision.gameObject);
            TakeDamage(damage);
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth > 0) return;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (killSound != null && StageManager.Instance != null)
        {
            StageManager.Instance.PlaySE(killSound, killVolume);
        }

        Debug.Log("敵を撃破！");
        StageManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
