using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage00MeteorSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.95f;
    [SerializeField] private float minSpawnInterval = 0.35f;
    [SerializeField] private float spawnY = 5.4f;
    [SerializeField] private float minFallSpeed = 1.7f;
    [SerializeField] private float maxFallSpeed = 3.0f;
    [SerializeField] private float minSize = 0.45f;
    [SerializeField] private float maxSize = 0.7f;
    [SerializeField] private int baseMinHealth = 1;
    [SerializeField] private int baseMaxHealth = 2;
    [SerializeField] private int maxMeteorsPerWave = 4;

    private float spawnTimer;
    private float spawnXLimit = 2.1f;
    private float elapsedTime;
    private bool isSpawning = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapStage00()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene00") return;
        if (FindObjectOfType<Stage00MeteorSpawner>() != null) return;

        GameObject spawner = new GameObject("Stage00MeteorSpawner");
        spawner.AddComponent<Stage00MeteorSpawner>();
    }

    private void Start()
    {
        spawnTimer = 1.8f;
        UpdateSpawnRange();
    }

    private void Update()
    {
        if (!isSpawning) return;

        elapsedTime += Time.deltaTime;
        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        SpawnMeteorWave();
        spawnTimer = GetCurrentSpawnInterval();
    }

    private void SpawnMeteorWave()
    {
        int waveCount = GetCurrentWaveCount();
        for (int i = 0; i < waveCount; i++)
        {
            SpawnMeteor();
        }
    }

    private void SpawnMeteor()
    {
        float x = Random.Range(-spawnXLimit, spawnXLimit);
        int health = CreateMeteorHealth();
        float speed = Random.Range(minFallSpeed, maxFallSpeed);
        float size = Random.Range(minSize, maxSize);

        GameObject meteor = new GameObject("SmallMeteorStage00");
        meteor.transform.position = new Vector3(x, spawnY, 0f);
        meteor.AddComponent<SmallMeteorStage00>().Initialize(health, speed, size);
    }

    private float GetCurrentSpawnInterval()
    {
        float difficulty = Mathf.Clamp01(elapsedTime / 60f);
        return Mathf.Lerp(spawnInterval, minSpawnInterval, difficulty);
    }

    private int GetCurrentWaveCount()
    {
        float difficulty = Mathf.Clamp01(elapsedTime / 50f);
        int count = 1 + Mathf.FloorToInt(difficulty * (maxMeteorsPerWave - 1));

        if (Random.value < difficulty * 0.6f)
        {
            count++;
        }

        return Mathf.Clamp(count, 1, maxMeteorsPerWave);
    }

    private int CreateMeteorHealth()
    {
        int bulletCount = 1;
        PlayerControllerStage00 stage00Player = FindObjectOfType<PlayerControllerStage00>();
        if (stage00Player != null) bulletCount = Mathf.Max(1, stage00Player.bulletCount);

        int bulletsPerPowerLevel = stage00Player != null ? Mathf.Max(1, stage00Player.bulletsPerPowerLevel) : 15;
        int powerLevel = Mathf.Max(0, (bulletCount - 1) / bulletsPerPowerLevel);
        int timeBonus = Mathf.FloorToInt(elapsedTime / 5f);
        int minHealth = baseMinHealth + powerLevel * 3 + timeBonus * 3;
        int maxHealth = baseMaxHealth + powerLevel * 10 + timeBonus * 10;
        minHealth = Mathf.Max(1, Mathf.RoundToInt(minHealth * 0.9f));
        maxHealth = Mathf.Max(minHealth, Mathf.RoundToInt(maxHealth * 0.9f));

        return Random.Range(minHealth, maxHealth + 1);
    }

    private void UpdateSpawnRange()
    {
        PlayerControllerStage00 stage00Player = FindObjectOfType<PlayerControllerStage00>();
        if (stage00Player != null)
        {
            spawnXLimit = stage00Player.xLimit;
            return;
        }

        PlayerController basePlayer = FindObjectOfType<PlayerController>();
        if (basePlayer != null) spawnXLimit = basePlayer.xLimit;
    }

    public void SetSpawningEnabled(bool enabled)
    {
        isSpawning = enabled;
    }
}
