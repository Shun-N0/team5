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
    private float timer;
    private bool hasFirstShotFired = false; // ★追加：最初の1発を撃ったかどうかのフラグ

    public enum AttackPattern
    {
        Straight, // まっすぐ1発
        ThreeWay, // 左右に広がる3方向弾
        Aimed,    // プレイヤーを狙い撃ち
        Burst     // 短い間隔での連射
    }

    [Header("攻撃パターン設定")]
    public AttackPattern attackPattern = AttackPattern.Straight; 
    public float spreadAngle = 20f;   
    public int burstCount = 3;        
    public float burstDelay = 0.12f;  

    [Header("弾の設定")]
    public float minShotInterval = 1.0f;
    public float maxShotInterval = 3.0f;
    private float currentShotInterval;
    public float bulletSpeed = 3f;
    private float currentBulletSpeed;

    [Header("弾の大きさ設定")]
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

    [Header("エフェクト設定")]
    public GameObject explosionPrefab; 

    [Header("アイテムドロップ設定")]
    public GameObject itemPrefab; 
    [Range(0f, 1f)] public float dropChance = 0.2f; 

    void Start()
    {
        currentHealth = maxHealth;
        InitializeEnemy();
        // ここではまだ撃たず、Updateで画面内判定を待ちます
        timer = 0f;
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
            // ★追加：画面に入った瞬間、まだ1発目も撃っていない場合
            if (!hasFirstShotFired)
            {
                Shoot();
                hasFirstShotFired = true; // フラグを立てる
                timer = 0f; // インターバルのカウントを開始
                // 次のインターバルをランダムに決める
                currentShotInterval = Random.Range(minShotInterval, maxShotInterval);
            }
            else
            {
                // 通常のインターバル射撃
                timer += Time.deltaTime;
                if (timer > currentShotInterval)
                {
                    Shoot();
                    timer = 0;
                    currentShotInterval = Random.Range(minShotInterval, maxShotInterval);
                }
            }
        }
    }

    bool IsOnScreen()
    {
        // カメラの視界（少し余裕を持って、完全に姿が見えてから判定）
        float cameraTop = Camera.main.transform.position.y + Camera.main.orthographicSize;
        return transform.position.y < (cameraTop - 0.2f); // 0.2ほど内側に入ったらON
    }

    public void Shoot()
    {
        if (enemyBulletPrefab == null) return;

        switch (attackPattern)
        {
            case AttackPattern.ThreeWay:
                FireBullet(RotateDir(Vector2.down, -spreadAngle));
                FireBullet(Vector2.down);
                FireBullet(RotateDir(Vector2.down, spreadAngle));
                break;
            case AttackPattern.Aimed:
                FireBullet(GetAimDirection());
                break;
            case AttackPattern.Burst:
                StartCoroutine(BurstFire());
                break;
            default:
                FireBullet(Vector2.down);
                break;
        }
    }

    void FireBullet(Vector2 fireDirection)
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        bullet.transform.localScale = new Vector3(currentBulletScale, currentBulletScale, 1);
        bullet.SendMessage("SetSpeed", currentBulletSpeed, SendMessageOptions.DontRequireReceiver);
        bullet.SendMessage("SetDirection", fireDirection, SendMessageOptions.DontRequireReceiver);
    }

    System.Collections.IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            FireBullet(Vector2.down);
            yield return new WaitForSeconds(burstDelay);
        }
    }

    Vector2 RotateDir(Vector2 dir, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
    }

    Vector2 GetAimDirection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Vector2.down;
        return ((Vector2)(player.transform.position - transform.position)).normalized;
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

        currentBulletSpeed = bulletSpeed;
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

        if (itemPrefab != null && Random.value < dropChance)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (killSound != null && StageManager.Instance != null) StageManager.Instance.PlaySE(killSound, killVolume);

        StageManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject);
    }
}