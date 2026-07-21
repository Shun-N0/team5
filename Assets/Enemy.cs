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

    // ★追加：攻撃パターンの種類
    // デフォルトは Straight（まっすぐ1発）なので、既存の敵の挙動は一切変わりません
    public enum AttackPattern
    {
        Straight, // まっすぐ1発（従来通り）
        ThreeWay, // 左右に広がる3方向弾
        Aimed,    // プレイヤーを狙い撃ち
        Burst     // 短い間隔での連射
    }

    [Header("攻撃パターン設定")]
    public AttackPattern attackPattern = AttackPattern.Straight; // 発射パターン
    public float spreadAngle = 20f;   // ThreeWay時、真下から左右へ広げる角度（度）
    public int burstCount = 3;        // Burst時に連射する弾数
    public float burstDelay = 0.12f;  // Burst時の弾と弾の間隔（秒）

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

    // ★追加：アイテムドロップ設定
    [Header("アイテムドロップ設定")]
    public GameObject itemPrefab; // 落としたいアイテムのプレハブ
    [Range(0f, 1f)] public float dropChance = 0.2f; // アイテムが落ちる確率 (0.2 = 20%)

    void Start()
    {
        currentHealth = maxHealth;
        InitializeEnemy();
        timer = Random.Range(0f, currentShotInterval);
    }

    void Update()
    {
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
        if (enemyBulletPrefab == null) return;

        // 攻撃パターンごとに発射方法を切り替える
        switch (attackPattern)
        {
            case AttackPattern.ThreeWay:
                // 真下を中心に、左右へ spreadAngle 度ずつ広げた3発を撃つ
                FireBullet(RotateDir(Vector2.down, -spreadAngle));
                FireBullet(Vector2.down);
                FireBullet(RotateDir(Vector2.down, spreadAngle));
                break;

            case AttackPattern.Aimed:
                // プレイヤーの位置を狙って1発撃つ
                FireBullet(GetAimDirection());
                break;

            case AttackPattern.Burst:
                // 短い間隔で連射する（時間差発射のためコルーチンを使う）
                StartCoroutine(BurstFire());
                break;

            default: // Straight（従来通り：まっすぐ1発）
                FireBullet(Vector2.down);
                break;
        }
    }

    // 指定した方向へ弾を1発撃つ共通処理
    void FireBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        bullet.transform.localScale = new Vector3(currentBulletScale, currentBulletScale, 1);
        bullet.SendMessage("SetSpeed", currentBulletSpeed, SendMessageOptions.DontRequireReceiver);
        // 弾が方向指定に対応していれば向きを設定する（未対応の弾は無視される）
        bullet.SendMessage("SetDirection", direction, SendMessageOptions.DontRequireReceiver);
    }

    // Burst：burstCount 発を burstDelay 間隔で真下へ連射する
    System.Collections.IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            FireBullet(Vector2.down);
            yield return new WaitForSeconds(burstDelay);
        }
    }

    // ベクトルを angle 度だけ回転させる（ThreeWayの拡散に使用）
    Vector2 RotateDir(Vector2 dir, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
    }

    // プレイヤーの方向を求める（見つからなければ真下）
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

        // ★追加：撃破された瞬間にアイテムを生成する（確率判定）
        if (itemPrefab != null && Random.value < dropChance)
        {
            // アイテムをその場に生成
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

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