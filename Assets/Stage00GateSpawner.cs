using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage00GateSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1.6f;
    [SerializeField] private float spawnY = 5.2f;
    [SerializeField] private float gateHeight = 0.8f;
    [SerializeField] private float gateFallSpeed = 2.4f;
    [SerializeField] private int minGateValue = 1;
    [SerializeField] private int maxGateValue = 8;
    [SerializeField] private int maxMultiplyValue = 3;
    [SerializeField] private int maxDivideValue = 3;

    private float spawnTimer;
    private float laneCenterX = 1.05f;
    private float laneWidth = 2.1f;
    private bool isSpawning = true;
    private int gatePairCount;
    private int multiplySlotA;
    private int multiplySlotB;
    private int bestPossibleBulletCount = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapStage00()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene00") return;

        RemoveStage00LegacyEnemies();
        EnsureStage00Player();

        if (FindObjectOfType<Stage00GateSpawner>() != null) return;
        GameObject spawner = new GameObject("Stage00GateSpawner");
        spawner.AddComponent<Stage00GateSpawner>();
    }

    private void Start()
    {
        spawnTimer = 0f;
        ChooseMultiplySlots();
        UpdateLaneSize();
    }

    private void Update()
    {
        if (!isSpawning) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        SpawnGatePair();
        spawnTimer = spawnInterval;
    }

    private void SpawnGatePair()
    {
        BulletGateStage00.GateOperation positiveOperation = ShouldSpawnMultiplyGate()
            ? BulletGateStage00.GateOperation.Multiply
            : BulletGateStage00.GateOperation.Add;
        BulletGateStage00.GateOperation negativeOperation = Random.value < 0.65f
            ? BulletGateStage00.GateOperation.Subtract
            : BulletGateStage00.GateOperation.Divide;

        int positiveValue = CreateGateValue(positiveOperation);
        int negativeValue = CreateGateValue(negativeOperation);
        bool isPlusLeft = Random.value < 0.5f;

        BulletGateStage00.GateOperation leftOperation = isPlusLeft ? positiveOperation : negativeOperation;
        BulletGateStage00.GateOperation rightOperation = isPlusLeft ? negativeOperation : positiveOperation;
        int leftValue = isPlusLeft ? positiveValue : negativeValue;
        int rightValue = isPlusLeft ? negativeValue : positiveValue;

        SpawnGate(-laneCenterX, leftOperation, leftValue);
        SpawnGate(laneCenterX, rightOperation, rightValue);
        ApplyBestPossiblePositiveGate(positiveOperation, positiveValue);
        gatePairCount++;
    }

    private void ApplyBestPossiblePositiveGate(BulletGateStage00.GateOperation operation, int value)
    {
        if (operation == BulletGateStage00.GateOperation.Multiply)
        {
            bestPossibleBulletCount = Mathf.Max(1, bestPossibleBulletCount * value);
            return;
        }

        bestPossibleBulletCount = Mathf.Max(1, bestPossibleBulletCount + value);
    }

    private void ChooseMultiplySlots()
    {
        multiplySlotA = Random.Range(0, 4);
        do
        {
            multiplySlotB = Random.Range(0, 4);
        }
        while (multiplySlotB == multiplySlotA);
    }

    private bool ShouldSpawnMultiplyGate()
    {
        return gatePairCount < 4 && (gatePairCount == multiplySlotA || gatePairCount == multiplySlotB);
    }

    private int CreateGateValue(BulletGateStage00.GateOperation operation)
    {
        if (operation == BulletGateStage00.GateOperation.Multiply)
        {
            return Random.Range(2, Mathf.Max(2, maxMultiplyValue) + 1);
        }

        if (operation == BulletGateStage00.GateOperation.Divide)
        {
            return Random.Range(2, Mathf.Max(2, maxDivideValue) + 1);
        }

        return Random.Range(minGateValue, maxGateValue + 1);
    }

    private void SpawnGate(float x, BulletGateStage00.GateOperation operation, int value)
    {
        bool isPositiveGate = operation == BulletGateStage00.GateOperation.Add || operation == BulletGateStage00.GateOperation.Multiply;
        GameObject gate = new GameObject(isPositiveGate ? "PositiveGate" : "NegativeGate");
        gate.transform.position = new Vector3(x, spawnY, 0f);
        gate.AddComponent<BulletGateStage00>().Initialize(operation, value, gateFallSpeed, new Vector2(laneWidth, gateHeight));
    }

    private void UpdateLaneSize()
    {
        float playerXLimit = 2.1f;

        PlayerControllerStage00 stage00Player = FindObjectOfType<PlayerControllerStage00>();
        if (stage00Player != null)
        {
            playerXLimit = stage00Player.xLimit;
        }
        else
        {
            PlayerController basePlayer = FindObjectOfType<PlayerController>();
            if (basePlayer != null) playerXLimit = basePlayer.xLimit;
        }

        laneWidth = playerXLimit;
        laneCenterX = playerXLimit * 0.5f;
    }

    private static void EnsureStage00Player()
    {
        PlayerControllerStage00 existingStage00Player = FindObjectOfType<PlayerControllerStage00>();
        if (existingStage00Player != null)
        {
            PlayerController existingBasePlayer = existingStage00Player.GetComponent<PlayerController>();
            if (existingBasePlayer != null) Destroy(existingBasePlayer);
            return;
        }

        PlayerController basePlayer = FindObjectOfType<PlayerController>();
        if (basePlayer == null) return;

        PlayerControllerStage00 stage00Player = basePlayer.gameObject.AddComponent<PlayerControllerStage00>();
        stage00Player.speed = basePlayer.speed;
        stage00Player.xLimit = basePlayer.xLimit;
        stage00Player.yMin = basePlayer.yMin + 0.85f;
        stage00Player.yMax = basePlayer.yMax;
        stage00Player.bulletPrefab = basePlayer.bulletPrefab;
        stage00Player.shotInterval = basePlayer.shotInterval;
        stage00Player.attackPower = basePlayer.attackPower;
        stage00Player.lives = basePlayer.lives;
        stage00Player.lifeIcons = basePlayer.lifeIcons;
        stage00Player.invincibleDuration = basePlayer.invincibleDuration;
        stage00Player.blinkInterval = basePlayer.blinkInterval;
        stage00Player.shotSound = basePlayer.shotSound;
        stage00Player.damageSound = basePlayer.damageSound;
        stage00Player.explosionSound = basePlayer.explosionSound;
        stage00Player.shotVolume = basePlayer.shotVolume;
        stage00Player.damageVolume = basePlayer.damageVolume;
        stage00Player.explosionVolume = basePlayer.explosionVolume;
        stage00Player.gameOverDelay = basePlayer.gameOverDelay;
        stage00Player.evolvedSprite = basePlayer.evolvedSprite;

        Destroy(basePlayer);
    }

    private static void RemoveStage00LegacyEnemies()
    {
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null) Destroy(enemySpawner.gameObject);

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }

        EnemyBullet[] enemyBullets = FindObjectsOfType<EnemyBullet>();
        foreach (EnemyBullet enemyBullet in enemyBullets)
        {
            if (enemyBullet != null) Destroy(enemyBullet.gameObject);
        }

        WarshipFormation[] formations = FindObjectsOfType<WarshipFormation>();
        foreach (WarshipFormation formation in formations)
        {
            if (formation != null) Destroy(formation.gameObject);
        }

        WarshipUnit[] units = FindObjectsOfType<WarshipUnit>();
        foreach (WarshipUnit unit in units)
        {
            if (unit != null) Destroy(unit.gameObject);
        }

        Boss boss = FindObjectOfType<Boss>();
        if (boss != null) Destroy(boss.gameObject);
    }

    public void SetSpawningEnabled(bool enabled)
    {
        isSpawning = enabled;
    }

    public int GetBestPossibleBulletCount()
    {
        return Mathf.Max(1, bestPossibleBulletCount);
    }
}
