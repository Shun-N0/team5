using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // 外部からこのスポナーにアクセスできるようにする（重要！）
    public static EnemySpawner Instance { get; private set; }

    public GameObject enemyPrefab;         
    public GameObject triangleEnemyPrefab; 

    [Header("耐久敵設定")]
    public GameObject tankEnemyPrefab;
    [Range(0f, 1f)] public float tankEnemySpawnChance = 0.2f;

    [Header("スタン敵設定")]
    public GameObject stunEnemyPrefab;
    [Range(0f, 1f)] public float stunEnemySpawnChance = 0.15f;
    public int stunEnemyMaxCount = 0;             // このステージで出す最大数（0 = 無制限）

    [Header("バリアアイテムを落とす敵設定")]
    public GameObject shieldEnemyPrefab;          // バリア(シールド)アイテムを確定で落とす敵
    public float shieldEnemySpawnInterval = 15f;  // 出現する間隔（秒）
    public float shieldEnemyInitialDelay = 3f;    // 最初に出現するまでの待ち時間（秒）
    public int shieldEnemyMaxCount = 0;           // このステージで出す最大数（0 = 無制限）

    [Header("撃破時アイテムドロップ設定")]
    public GameObject defeatDropItemPrefab;        // 敵を倒した時に落とすアイテム
    [Range(0f, 1f)] public float defeatDropChance = 0f;

    [Header("軍艦編隊設定")]
    public GameObject warshipFormationPrefab;
    public float blueWarshipSpawnInterval = 18f;
    public float blueWarshipInitialDelay = 9f;
    public GameObject redWarshipFormationPrefab;
    public float redWarshipSpawnInterval = 16f;
    public float redWarshipInitialDelay = 4f;

    [Header("初期スポーン設定")]
    public float initialSpawnInterval = 3.0f; 

    [Header("敵数管理")]
    public int targetEnemyCount = 5; 

    [Header("難易度上昇設定")]
    public float minSpawnInterval = 0.8f;     
    public float difficultyInterval = 15.0f;  
    public float spawnIntervalDecrement = 0.3f; 

    private float currentSpawnInterval;  
    private float spawnTimer = 0f;       
    private float difficultyTimer = 0f;  
    private float blueWarshipTimer;
    private float redWarshipTimer;
    private float shieldEnemyTimer;

    // ★追加：アイテム敵をこれまでに何体出したかのカウンタ（最大数制限に使用）
    private int shieldEnemySpawnedCount = 0;
    private int stunEnemySpawnedCount = 0;

    // ★追加：ボスの間、生成を止めるためのフラグ
    private bool isSpawningStopped = false;

    void Awake()
    {
        // シングルトン設定（外部から呼びやすくするため）
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        blueWarshipTimer = blueWarshipInitialDelay;
        redWarshipTimer = redWarshipInitialDelay;
        shieldEnemyTimer = shieldEnemyInitialDelay;
    }

    void Update()
    {
        // ★追加：生成が停止されている場合は、何もしない（他のシーンでは常にfalseなので干渉しません）
        if (isSpawningStopped) return;

        // --- 難易度上昇タイマー ---
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyInterval)
        {
            difficultyTimer = 0f;
            currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnIntervalDecrement, minSpawnInterval);
            Debug.Log("難易度アップ！出現間隔: " + currentSpawnInterval + "秒");
        }

        // --- 敵のスポーンタイマー ---
        int currentEnemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        // WarshipUnitのカウント（もしスクリプトが存在しなければ無視されます）
        // currentEnemyCount += FindObjectsByType<WarshipUnit>(FindObjectsSortMode.None).Length;

        float adjustedInterval = currentEnemyCount < targetEnemyCount
            ? currentSpawnInterval * 0.3f  
            : currentSpawnInterval;        

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= adjustedInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }

        UpdateWarshipSpawns();
        UpdateShieldEnemySpawn();
    }

    // ★追加：バリアアイテムを落とす敵を一定間隔で出現させる
    private void UpdateShieldEnemySpawn()
    {
        if (shieldEnemyPrefab == null) return;

        // ★追加：最大数に達していたら、もう出さない（0のときは無制限）
        if (shieldEnemyMaxCount > 0 && shieldEnemySpawnedCount >= shieldEnemyMaxCount) return;

        shieldEnemyTimer -= Time.deltaTime;
        if (shieldEnemyTimer <= 0f)
        {
            // 敵自身のStart()で画面上部のランダムな位置に配置されるため、原点で生成してよい
            GameObject enemy = Instantiate(shieldEnemyPrefab, Vector3.zero, Quaternion.identity);
            ApplyDefeatDropSetting(enemy);
            shieldEnemyTimer = shieldEnemySpawnInterval;
            shieldEnemySpawnedCount++;
            Debug.Log("バリアアイテムを持った敵が出現！");
        }
    }

    // ★追加：外部（StageManagerなど）から生成を止めるための命令
    public void StopSpawning()
    {
        isSpawningStopped = true;
        Debug.Log("ボス出現のため、ザコ敵の生成を停止しました。");
    }

    void SpawnEnemy()
    {
        if (tankEnemyPrefab != null && Random.value < tankEnemySpawnChance)
        {
            GameObject enemy = Instantiate(tankEnemyPrefab, Vector3.zero, Quaternion.identity);
            ApplyDefeatDropSetting(enemy);
            return;
        }

        // ★追加：最大数に達していないときだけスタン敵を出す（stunEnemyMaxCount が0なら無制限）
        if (stunEnemyPrefab != null && Random.value < stunEnemySpawnChance
            && (stunEnemyMaxCount <= 0 || stunEnemySpawnedCount < stunEnemyMaxCount))
        {
            GameObject enemy = Instantiate(stunEnemyPrefab, Vector3.zero, Quaternion.identity);
            ApplyDefeatDropSetting(enemy);
            stunEnemySpawnedCount++;
            return;
        }

        GameObject prefabToSpawn = Random.value > 0.5f ? enemyPrefab : triangleEnemyPrefab;

        if (prefabToSpawn != null)
        {
            GameObject enemy = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
            ApplyDefeatDropSetting(enemy);
        }
    }

    private void ApplyDefeatDropSetting(GameObject enemyObject)
    {
        if (defeatDropItemPrefab == null || defeatDropChance <= 0f) return;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.SetItemDrop(defeatDropItemPrefab, defeatDropChance);
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
