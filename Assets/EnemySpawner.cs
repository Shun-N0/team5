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
            Instantiate(tankEnemyPrefab, Vector3.zero, Quaternion.identity);
            return;
        }

        if (stunEnemyPrefab != null && Random.value < stunEnemySpawnChance)
        {
            Instantiate(stunEnemyPrefab, Vector3.zero, Quaternion.identity);
            return;
        }

        GameObject prefabToSpawn = Random.value > 0.5f ? enemyPrefab : triangleEnemyPrefab;

        if (prefabToSpawn != null)
        {
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