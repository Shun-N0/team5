using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MechaJellyfishBoss : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private int maxHealth = 45;
    [SerializeField] private int scoreValue = 1000;
    [SerializeField] private float stopY = 3.2f;
    [SerializeField] private float moveSpeed = 1.4f;
    [SerializeField] private float attackInterval = 1.6f;

    [Header("弾設定")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float bulletSpeed = 4.2f;
    [SerializeField] private float bulletScale = 0.6f;
    [SerializeField] private int burstCount = 4;
    [SerializeField] private float burstDelay = 0.16f;
    [SerializeField] private float diagonalAngle = 14f;

    [Header("視界妨害")]
    [SerializeField] private float blackoutDuration = 0.3f;

    [Header("体力ゲージ")]
    [SerializeField] private Vector2 healthGaugeSize = new Vector2(24f, 260f);
    [SerializeField] private Vector2 healthGaugeOffset = new Vector2(24f, 0f);
    [SerializeField] private Color healthGaugeBackgroundColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private Color healthGaugeFillColor = new Color(1f, 0.12f, 0.18f, 0.9f);

    private int currentHealth;
    private bool isReady;
    private Coroutine attackCoroutine;
    private Coroutine blackoutCoroutine;
    private GameObject blackoutObject;
    private GameObject healthGaugeObject;
    private RectTransform healthGaugeFillRect;
    private SampleScene01BossHitFlash hitFlash;

    void Awake()
    {
        hitFlash = GetComponent<SampleScene01BossHitFlash>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        CreateBlackoutOverlay();
        CreateHealthGauge();
        UpdateHealthGauge();
    }

    void Update()
    {
        if (isReady) return;

        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        if (transform.position.y <= stopY)
        {
            isReady = true;
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    void OnDestroy()
    {
        if (healthGaugeObject != null)
        {
            Destroy(healthGaugeObject);
        }
    }

    IEnumerator AttackRoutine()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            yield return StartCoroutine(FireBurstRoutine());
        }
    }

    IEnumerator FireBurstRoutine()
    {
        ShowBlackout();

        for (int i = 0; i < burstCount; i++)
        {
            FireVolley();
            yield return new WaitForSeconds(burstDelay);
        }
    }

    void FireVolley()
    {
        // 正面、左斜め、右斜めへ同時に撃つ。
        FireBullet(0f);
        FireBullet(diagonalAngle);
        FireBullet(-diagonalAngle);
    }

    void FireBullet(float angle)
    {
        if (enemyBulletPrefab == null) return;

        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, rotation);
        bullet.transform.localScale = Vector3.one * bulletScale;
        bullet.SendMessage("SetSpeed", bulletSpeed, SendMessageOptions.DontRequireReceiver);
    }

    void CreateBlackoutOverlay()
    {
        blackoutObject = new GameObject("Boss Blackout Overlay");
        blackoutObject.transform.SetParent(transform, false);

        Canvas canvas = blackoutObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        blackoutObject.AddComponent<CanvasScaler>();
        blackoutObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("Black Screen");
        imageObject.transform.SetParent(blackoutObject.transform, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        blackoutObject.SetActive(false);
    }

    void ShowBlackout()
    {
        if (blackoutObject == null) return;

        if (blackoutCoroutine != null)
        {
            StopCoroutine(blackoutCoroutine);
        }

        blackoutCoroutine = StartCoroutine(BlackoutRoutine());
    }

    IEnumerator BlackoutRoutine()
    {
        blackoutObject.SetActive(true);
        yield return new WaitForSeconds(blackoutDuration);
        blackoutObject.SetActive(false);
        blackoutCoroutine = null;
    }

    void CreateHealthGauge()
    {
        healthGaugeObject = new GameObject("Boss Health Gauge");

        Canvas canvas = healthGaugeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = healthGaugeObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        healthGaugeObject.AddComponent<GraphicRaycaster>();

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(healthGaugeObject.transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = healthGaugeBackgroundColor;

        RectTransform backgroundRect = backgroundImage.rectTransform;
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = healthGaugeOffset;
        backgroundRect.sizeDelta = healthGaugeSize;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = healthGaugeFillColor;

        healthGaugeFillRect = fillImage.rectTransform;
        healthGaugeFillRect.anchorMin = new Vector2(0f, 0f);
        healthGaugeFillRect.anchorMax = new Vector2(1f, 0f);
        healthGaugeFillRect.pivot = new Vector2(0.5f, 0f);
        healthGaugeFillRect.anchoredPosition = new Vector2(0f, 3f);
        healthGaugeFillRect.sizeDelta = new Vector2(-6f, healthGaugeSize.y - 6f);
    }

    void UpdateHealthGauge()
    {
        if (healthGaugeFillRect == null) return;

        float healthRate = maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;
        float fillHeight = (healthGaugeSize.y - 6f) * Mathf.Clamp01(healthRate);
        healthGaugeFillRect.sizeDelta = new Vector2(-6f, fillHeight);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet")) return;

        Destroy(collision.gameObject);
        TakeDamage(1);
    }

    void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthGauge();
        hitFlash?.Play();

        if (currentHealth > 0) return;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        if (blackoutObject != null)
        {
            Destroy(blackoutObject);
        }

        if (healthGaugeObject != null)
        {
            Destroy(healthGaugeObject);
        }

        StageManager.Instance?.AddScore(scoreValue);
        StageManager.Instance?.TriggerClear();
        Destroy(gameObject);
    }
}
