using UnityEngine;
using UnityEngine.SceneManagement;

public class SmallMeteorStage00 : MonoBehaviour
{
    private const float DestroyY = -5.8f;
    private static Sprite meteorSprite;
    private static Sprite flameTrailSprite;

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float fallSpeed = 2.2f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float size = 0.55f;

    private int currentHealth;
    private Transform flameTrailTransform;

    public void Initialize(int health, float speed, float meteorSize)
    {
        maxHealth = Mathf.Max(1, health);
        fallSpeed = speed;
        size = meteorSize;
        currentHealth = maxHealth;
        SetupVisuals();
        SetupCollision();
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        SetupVisuals();
        SetupCollision();
    }

    private void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        UpdateFlameTrail();

        if (transform.position.y < DestroyY)
        {
            TriggerGameOver();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            currentHealth -= Mathf.Max(1, bullet.attackPower);
            Destroy(collision.gameObject);

            if (currentHealth <= 0) Destroy(gameObject);
            return;
        }

        if (collision.GetComponent<PlayerControllerStage00>() != null)
        {
            TriggerGameOver();
            Destroy(gameObject);
        }
    }

    private void TriggerGameOver()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.TriggerGameOver();
        }
        else
        {
            SceneManager.LoadScene("GameOverScene");
        }
    }

    private void SetupCollision()
    {
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle == null) circle = gameObject.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 0.5f;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void SetupVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetMeteorSprite();
        sr.color = new Color(0.62f, 0.55f, 0.48f);
        sr.sortingOrder = 18;
        transform.localScale = Vector3.one * size;
        SetupFlameTrail();
    }

    private static Sprite GetMeteorSprite()
    {
        if (meteorSprite != null) return meteorSprite;

        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = Vector2.one * (textureSize - 1) * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                float angleNoise = Mathf.Sin(Mathf.Atan2(point.y - center.y, point.x - center.x) * 7f) * 3f;
                float radius = 21f + angleNoise;
                float flameRadius = radius + 9f;

                if (distance > flameRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                if (distance > radius)
                {
                    float flamePower = Mathf.InverseLerp(flameRadius, radius, distance);
                    Color flameColor = Color.Lerp(
                        new Color(1f, 0.18f, 0f, 0.15f),
                        new Color(1f, 0.82f, 0.1f, 0.9f),
                        flamePower
                    );

                    if (point.y > center.y + 6f)
                    {
                        flameColor.a *= 0.45f;
                    }

                    texture.SetPixel(x, y, flameColor);
                    continue;
                }

                float shade = Mathf.InverseLerp(radius, 0f, distance);
                Color color = Color.Lerp(new Color(0.38f, 0.32f, 0.28f), new Color(0.78f, 0.7f, 0.62f), shade);

                if (Vector2.Distance(point, center + new Vector2(-9f, 7f)) < 5f ||
                    Vector2.Distance(point, center + new Vector2(10f, -8f)) < 6f ||
                    Vector2.Distance(point, center + new Vector2(6f, 11f)) < 4f)
                {
                    color *= 0.55f;
                }

                texture.SetPixel(x, y, color);
            }
        }

        DrawFlameTail(texture, center);

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        meteorSprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, textureSize);
        return meteorSprite;
    }

    private static void DrawFlameTail(Texture2D texture, Vector2 center)
    {
        for (int y = 36; y < 63; y++)
        {
            for (int x = 14; x < 50; x++)
            {
                Vector2 point = new Vector2(x, y);
                float horizontal = Mathf.Abs(point.x - center.x);
                float vertical = y - 35f;
                float tailWidth = Mathf.Lerp(15f, 2f, vertical / 28f);
                if (horizontal > tailWidth) continue;

                float alpha = Mathf.Lerp(0.55f, 0f, vertical / 28f);
                Color flame = Color.Lerp(new Color(1f, 0.95f, 0.25f, alpha), new Color(1f, 0.25f, 0f, alpha), horizontal / Mathf.Max(1f, tailWidth));
                texture.SetPixel(x, y, flame);
            }
        }
    }

    private void SetupFlameTrail()
    {
        Transform existing = transform.Find("FlameTrail");
        GameObject trailObject = existing != null ? existing.gameObject : new GameObject("FlameTrail");
        trailObject.transform.SetParent(transform, false);
        flameTrailTransform = trailObject.transform;
        flameTrailTransform.localScale = new Vector3(1.45f, 2.05f, 1f);

        SpriteRenderer trailRenderer = trailObject.GetComponent<SpriteRenderer>();
        if (trailRenderer == null) trailRenderer = trailObject.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = GetFlameTrailSprite();
        trailRenderer.color = Color.white;
        trailRenderer.sortingOrder = 17;

        UpdateFlameTrail();
    }

    private void UpdateFlameTrail()
    {
        if (flameTrailTransform == null) return;

        flameTrailTransform.position = transform.position + Vector3.up * size * 0.28f;
        flameTrailTransform.rotation = Quaternion.identity;
    }

    private static Sprite GetFlameTrailSprite()
    {
        if (flameTrailSprite != null) return flameTrailSprite;

        const int textureWidth = 96;
        const int textureHeight = 128;
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        Vector2 center = new Vector2((textureWidth - 1) * 0.5f, 8f);

        for (int y = 0; y < textureHeight; y++)
        {
            float t = y / (float)(textureHeight - 1);
            float width = Mathf.Lerp(28f, 3f, t);
            float wave = Mathf.Sin(t * 16f) * 4f;

            for (int x = 0; x < textureWidth; x++)
            {
                float dx = Mathf.Abs(x - center.x - wave);
                if (dx > width)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edge = 1f - dx / width;
                float alpha = Mathf.Pow(edge, 1.15f) * Mathf.Lerp(0.95f, 0f, t);
                Color outer = new Color(1f, 0.12f, 0f, alpha);
                Color inner = new Color(1f, 0.95f, 0.12f, alpha);
                texture.SetPixel(x, y, Color.Lerp(outer, inner, edge));
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        flameTrailSprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0f), textureWidth);
        return flameTrailSprite;
    }
}
