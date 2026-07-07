using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MechaJellyfishBoss : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private int maxHealth = 150;
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
    [SerializeField] private float diagonalAngle = 28f;

    [Header("視界妨害")]
    [SerializeField] private float blackoutDuration = 0.3f;

    private int currentHealth;
    private bool isReady;
    private Coroutine attackCoroutine;
    private Coroutine blackoutCoroutine;
    private GameObject blackoutObject;

    void Start()
    {
        currentHealth = maxHealth;
        CreateBlackoutOverlay();
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet")) return;

        Destroy(collision.gameObject);
        TakeDamage(1);
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0) return;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        if (blackoutObject != null)
        {
            Destroy(blackoutObject);
        }

        StageManager.Instance?.AddScore(scoreValue);
        StageManager.Instance?.TriggerClear();
        Destroy(gameObject);
    }
}
