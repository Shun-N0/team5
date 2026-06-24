using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;         // 四角い敵のプレハブ
    public GameObject triangleEnemyPrefab; // 三角の敵のプレハブ

    [Header("耐久敵設定")]
    public GameObject tankEnemyPrefab;
    [Range(0f, 1f)] public float tankEnemySpawnChance = 0.2f;

    [Header("軍艦編隊設定")]
    public GameObject warshipFormationPrefab;
    public float blueWarshipSpawnInterval = 18f;
    public float blueWarshipInitialDelay = 9f;
    public GameObject redWarshipFormationPrefab;
    public float redWarshipSpawnInterval = 16f;
    public float redWarshipInitialDelay = 4f;

    [Header("初期スポーン設定")]
    public float initialSpawnInterval = 3.0f; // 最初の出現間隔（秒）

    [Header("敵数管理")]
    public int targetEnemyCount = 5; // 画面上に維持したい敵の数

    [Header("難易度上昇設定")]
    public float minSpawnInterval = 0.8f;     // 出現間隔の最小値（これ以上は速くならない）
    public float difficultyInterval = 15.0f;  // 何秒おきに難易度を上げるか
    public float spawnIntervalDecrement = 0.3f; // 1段階ごとに縮める秒数

    private float currentSpawnInterval;  // 現在の出現間隔
    private float spawnTimer = 0f;       // 次の敵を出すまでのタイマー
    private float difficultyTimer = 0f;  // 次の難易度上昇までのタイマー
    private float blueWarshipTimer;
    private float redWarshipTimer;

    void Start()
    {
        // 最初の出現間隔をセット
        currentSpawnInterval = initialSpawnInterval;
        blueWarshipTimer = blueWarshipInitialDelay;
        redWarshipTimer = redWarshipInitialDelay;
    }

    void Update()
    {
        // --- 難易度上昇タイマー ---
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyInterval)
        {
            difficultyTimer = 0f;
            // 出現間隔を短くする（minSpawnInterval を下回らないようにクランプ）
            currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnIntervalDecrement, minSpawnInterval);
            Debug.Log("難易度アップ！出現間隔: " + currentSpawnInterval + "秒");
        }

        // --- 敵のスポーンタイマー ---
        // 画面上の敵が少ないほどスポーン間隔を短くして敵数を一定に保つ
        int currentEnemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        currentEnemyCount += FindObjectsByType<WarshipUnit>(FindObjectsSortMode.None).Length;
        float adjustedInterval = currentEnemyCount < targetEnemyCount
            ? currentSpawnInterval * 0.3f  // 敵が少ないときは早めにスポーン
            : currentSpawnInterval;        // 十分いるときは通常間隔

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= adjustedInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }

        UpdateWarshipSpawns();
    }

    void SpawnEnemy()
    {
        if (tankEnemyPrefab != null && Random.value < tankEnemySpawnChance)
        {
            Instantiate(tankEnemyPrefab, Vector3.zero, Quaternion.identity);
            return;
        }

        // 出す敵の種類をランダムに選ぶ（四角か三角か）
        GameObject prefabToSpawn = Random.value > 0.5f ? enemyPrefab : triangleEnemyPrefab;

        if (prefabToSpawn != null)
        {
            // 敵を生成（位置はEnemyスクリプトのStartで決まるのでゼロ地点でOK）
            Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
        }
    }

    private void UpdateWarshipSpawns()
    {
        blueWarshipTimer -= Time.deltaTime;
        redWarshipTimer -= Time.deltaTime;

        WarshipFormation[] formations =
            FindObjectsByType<WarshipFormation>(FindObjectsSortMode.None);

        bool blueFormationExists = false;
        bool redFormationExists = false;

        foreach (WarshipFormation formation in formations)
        {
            if (formation.Type == WarshipFormation.FormationType.FiveShips)
                blueFormationExists = true;
            else if (formation.Type == WarshipFormation.FormationType.ThreeShips)
                redFormationExists = true;
        }

        if (!blueFormationExists &&
            blueWarshipTimer <= 0f &&
            warshipFormationPrefab != null)
        {
            Instantiate(warshipFormationPrefab, Vector3.zero, Quaternion.identity);
            blueWarshipTimer = blueWarshipSpawnInterval;
        }

        if (!redFormationExists &&
            redWarshipTimer <= 0f &&
            redWarshipFormationPrefab != null)
        {
            Instantiate(redWarshipFormationPrefab, Vector3.zero, Quaternion.identity);
            redWarshipTimer = redWarshipSpawnInterval;
        }
    }
}
