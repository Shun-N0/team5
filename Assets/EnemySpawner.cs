using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("ザコ敵のプレハブ")]
    public GameObject enemyPrefab;         
    public GameObject triangleEnemyPrefab; 

    [Header("耐久敵設定")]
    public GameObject tankEnemyPrefab;
    [Range(0f, 1f)] public float tankEnemySpawnChance = 0.2f;

    [Header("スタン敵設定")]
    public GameObject stunEnemyPrefab;
    public float stunEnemySpawnInterval = 20f; // 秒数管理
    public float stunEnemyInitialDelay = 5f;   
    public int stunEnemyMaxCount = 0;          

    [Header("バリアアイテムを落とす敵設定")]
    public GameObject shieldEnemyPrefab;          
    public float shieldEnemySpawnInterval = 25f;  
    public float shieldEnemyInitialDelay = 3f;    
    public int shieldEnemyMaxCount = 0;           

    [Header("撃破時アイテムドロップ設定")]
    public GameObject defeatDropItemPrefab;        
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
    private float stunEnemyTimer;

    private int shieldEnemySpawnedCount = 0;
    private int stunEnemySpawnedCount = 0;

    private bool isSpawningStopped = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        blueWarshipTimer = blueWarshipInitialDelay;
        redWarshipTimer = redWarshipInitialDelay;
        shieldEnemyTimer = shieldEnemyInitialDelay;
        stunEnemyTimer = stunEnemyInitialDelay;
    }

    void Update()
    {
        if (isSpawningStopped) return;

        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyInterval)
        {
            difficultyTimer = 0f;
            currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnIntervalDecrement, minSpawnInterval);
        }

        int currentEnemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        float adjustedInterval = currentEnemyCount < targetEnemyCount ? currentSpawnInterval * 0.3f : currentSpawnInterval;        

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= adjustedInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }

        UpdateWarshipSpawns();
        UpdateShieldEnemySpawn();
        UpdateStunEnemySpawn();
    }

    private void UpdateStunEnemySpawn()
    {
        if (stunEnemyPrefab == null) return;
        if (stunEnemyMaxCount > 0 && stunEnemySpawnedCount >= stunEnemyMaxCount) return;

        stunEnemyTimer -= Time.deltaTime;
        if (stunEnemyTimer <= 0f)
        {
            SpawnWithImmediateShoot(stunEnemyPrefab); 
            stunEnemyTimer = stunEnemySpawnInterval;
            stunEnemySpawnedCount++;
        }
    }

    private void UpdateShieldEnemySpawn()
    {
        if (shieldEnemyPrefab == null) return;
        if (shieldEnemyMaxCount > 0 && shieldEnemySpawnedCount >= shieldEnemyMaxCount) return;

        shieldEnemyTimer -= Time.deltaTime;
        if (shieldEnemyTimer <= 0f)
        {
            GameObject enemy = Instantiate(shieldEnemyPrefab, Vector3.zero, Quaternion.identity);
            ApplyDefeatDropSetting(enemy);
            shieldEnemyTimer = shieldEnemySpawnInterval;
            shieldEnemySpawnedCount++;
        }
    }

    public void StopSpawning() { isSpawningStopped = true; }
    public void ResumeSpawning() { isSpawningStopped = false; spawnTimer = 0f; difficultyTimer = 0f; }

    void SpawnEnemy()
    {
        // 1. 耐久敵の抽選
        if (tankEnemyPrefab != null && Random.value < tankEnemySpawnChance)
        {
            GameObject enemy = Instantiate(tankEnemyPrefab, Vector3.zero, Quaternion.identity);
            ApplyDefeatDropSetting(enemy);
            return;
        }

        // 2. 通常敵（四角か三角）の抽選
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
        if (enemy != null) enemy.SetItemDrop(defeatDropItemPrefab, defeatDropChance);
    }

    void SpawnWithImmediateShoot(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        go.SendMessage("Shoot", SendMessageOptions.DontRequireReceiver);
        ApplyDefeatDropSetting(go);
    }

    private void UpdateWarshipSpawns()
    {
        blueWarshipTimer -= Time.deltaTime;
        redWarshipTimer -= Time.deltaTime;
        WarshipFormation[] formations = FindObjectsByType<WarshipFormation>(FindObjectsSortMode.None);
        bool bExists = false; bool rExists = false;
        foreach (var f in formations) { if (f.Type == WarshipFormation.FormationType.FiveShips) bExists = true; else if (f.Type == WarshipFormation.FormationType.ThreeShips) rExists = true; }
        if (!bExists && blueWarshipTimer <= 0f && warshipFormationPrefab != null) { Instantiate(warshipFormationPrefab, Vector3.zero, Quaternion.identity); blueWarshipTimer = blueWarshipSpawnInterval; }
        if (!rExists && redWarshipTimer <= 0f && redWarshipFormationPrefab != null) { Instantiate(redWarshipFormationPrefab, Vector3.zero, Quaternion.identity); redWarshipTimer = redWarshipSpawnInterval; }
    }
}