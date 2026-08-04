using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage00BossSpawner : MonoBehaviour
{
    [SerializeField] private float bossSpawnDelay = 30f;
    [SerializeField] private float warningDuration = 3f;
    [SerializeField] private float expectedBossFightSeconds = 4.8f;
    [SerializeField] private float expectedBulletHitRate = 0.8f;
    [SerializeField] private float bossHealthRatioForPerfectRoute = 0.9f;
    [SerializeField] private int minimumBossHealth = 450;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 5.8f, 0f);

    private float timer;
    private bool spawned;
    private bool warningStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapStage00()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene00") return;
        if (FindObjectOfType<Stage00BossSpawner>() != null) return;

        GameObject spawner = new GameObject("Stage00BossSpawner");
        spawner.AddComponent<Stage00BossSpawner>();
    }

    // ★追加：ステージ0が始まった瞬間に実行される処理
    private void Start()
    {
        // 1. 止まった時間を動かす（リトライ対策）
        Time.timeScale = 1f;

        // 2. 今のシーン名を「SavedScene」という名前で保存する（これでリトライボタンが動くようになる）
        PlayerPrefs.SetString("SavedScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        
        Debug.Log("ステージ0：シーン名を保存しました - " + SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (spawned || warningStarted) return;

        timer += Time.deltaTime;
        if (timer < bossSpawnDelay) return;

        StartCoroutine(WarningThenSpawnBoss());
    }

    private IEnumerator WarningThenSpawnBoss()
    {
        warningStarted = true;

        // 各種生成機を止める（FindObjectOfTypeは重い処理ですが、ボス出現時1回だけなので許容します）
        var meteorSpawner = FindObjectOfType<Stage00MeteorSpawner>();
        if (meteorSpawner != null) meteorSpawner.SetSpawningEnabled(false);

        var gateSpawner = FindObjectOfType<Stage00GateSpawner>();
        if (gateSpawner != null) gateSpawner.SetSpawningEnabled(false);

        GameObject warningObject = CreateWarningUI();
        yield return new WaitForSeconds(warningDuration);

        if (warningObject != null) Destroy(warningObject);
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        spawned = true;

        PlayerControllerStage00 player = FindObjectOfType<PlayerControllerStage00>();
        Stage00GateSpawner gateSpawner = FindObjectOfType<Stage00GateSpawner>();
        int bossHealth = CalculateBalancedBossHealth(player, gateSpawner);

        GameObject boss = new GameObject("MeteorBossStage00");
        boss.transform.position = spawnPosition;
        boss.AddComponent<MeteorBossStage00>().Initialize(bossHealth);
    }

    private int CalculateBalancedBossHealth(PlayerControllerStage00 player, Stage00GateSpawner gateSpawner)
    {
        int perfectBulletCount = gateSpawner != null ? gateSpawner.GetBestPossibleBulletCount() : 1;
        if (player != null) perfectBulletCount = Mathf.Max(perfectBulletCount, player.bulletCount);

        int visibleBullets = Mathf.Min(perfectBulletCount, 10);
        int bulletsPerPowerLevel = player != null ? Mathf.Max(1, player.bulletsPerPowerLevel) : 15;
        int attackPower = player != null ? Mathf.Max(1, player.attackPower) : 1;
        int powerLevel = Mathf.Max(0, (perfectBulletCount - 1) / bulletsPerPowerLevel);
        float shotInterval = player != null ? Mathf.Max(0.05f, player.shotInterval) : 0.15f;
        int expectedShotCount = Mathf.Max(1, Mathf.FloorToInt(expectedBossFightSeconds / shotInterval));
        float perfectRouteDamage = visibleBullets * (attackPower + powerLevel) * expectedShotCount;

        float expectedHitDamage = perfectRouteDamage * expectedBulletHitRate;
        int calculatedHealth = Mathf.RoundToInt(expectedHitDamage * bossHealthRatioForPerfectRoute);
        return Mathf.Max(minimumBossHealth, calculatedHealth);
    }

    private GameObject CreateWarningUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Stage00WarningCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject warningObject = new GameObject("BossWarning");
        warningObject.transform.SetParent(canvas.transform, false);

        Image warningBackground = warningObject.AddComponent<Image>();
        warningBackground.color = new Color(1f, 0f, 0f, 0.18f);
        warningBackground.raycastTarget = false;

        RectTransform backgroundRect = warningBackground.rectTransform;
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(560f, 150f);

        GameObject textObject = new GameObject("WarningText");
        textObject.transform.SetParent(warningObject.transform, false);
        TextMeshProUGUI warningText = textObject.AddComponent<TextMeshProUGUI>();
        warningText.text = "WARNING";
        warningText.fontSize = 72f;
        warningText.fontStyle = FontStyles.Bold;
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.color = new Color(1f, 0.15f, 0.1f);

        RectTransform rect = warningText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(520f, 120f);

        // Stage00WarningBlink がプロジェクト内に存在する場合のみ追加
        warningObject.AddComponent<Stage00WarningBlink>();

        return warningObject;
    }
}