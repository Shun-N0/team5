using UnityEngine;
using UnityEngine.UI;

public class BossHealthGauge
{
    private readonly GameObject gaugeObject;
    private readonly RectTransform fillRect;
    private readonly Vector2 gaugeSize;

    private BossHealthGauge(GameObject gaugeObject, RectTransform fillRect, Vector2 gaugeSize)
    {
        this.gaugeObject = gaugeObject;
        this.fillRect = fillRect;
        this.gaugeSize = gaugeSize;
    }

    public static BossHealthGauge Create(string name)
    {
        Vector2 size = new Vector2(24f, 260f);
        Vector2 offset = new Vector2(24f, 0f);

        GameObject rootObject = new GameObject(name);

        Canvas canvas = rootObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = rootObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        rootObject.AddComponent<GraphicRaycaster>();

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(rootObject.transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.55f);

        RectTransform backgroundRect = backgroundImage.rectTransform;
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = offset;
        backgroundRect.sizeDelta = size;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(1f, 0.12f, 0.18f, 0.9f);

        RectTransform fill = fillImage.rectTransform;
        fill.anchorMin = new Vector2(0f, 0f);
        fill.anchorMax = new Vector2(1f, 0f);
        fill.pivot = new Vector2(0.5f, 0f);
        fill.anchoredPosition = new Vector2(0f, 3f);
        fill.sizeDelta = new Vector2(-6f, size.y - 6f);

        return new BossHealthGauge(rootObject, fill, size);
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (fillRect == null) return;

        float healthRate = maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;
        float fillHeight = (gaugeSize.y - 6f) * Mathf.Clamp01(healthRate);
        fillRect.sizeDelta = new Vector2(-6f, fillHeight);
    }

    public void Destroy()
    {
        if (gaugeObject != null)
        {
            Object.Destroy(gaugeObject);
        }
    }
}
