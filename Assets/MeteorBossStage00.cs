using UnityEngine;
using UnityEngine.SceneManagement;

public class MeteorBossStage00 : MonoBehaviour
{
    private const string BossSpriteResourcePath = "Stage00/MeteorBossStage00";
    private const string ClearSceneName = "Clear Game";
    private static Sprite bossSprite;
    private static Sprite hpBarSprite;
    private static Sprite shockwaveSprite;
    private static Sprite glowSprite;

    [SerializeField] private int maxHealth = 600;
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float horizontalSpeed = 1.1f;
    [SerializeField] private float xLimit = 1.25f;
    [SerializeField] private float size = 2.2f;
    [SerializeField] private float hpBarTopMargin = 0.35f;
    [SerializeField] private float hpBarHeight = 0.16f;
    [SerializeField] private float defeatSlowTimeScale = 0.16f;
    [SerializeField] private float defeatDuration = 3.1f;

    private int currentHealth;
    private float horizontalDirection = 1f;
    private SpriteRenderer bossRenderer;
    private SpriteRenderer hpFillRenderer;
    private Transform hpFillTransform;
    private Transform hpBarRoot;
    private float hpBarWidth = 4f;
    private float damageFeedbackTimer;
    private Vector3 baseScale;
    private bool isClearing;

    public void Initialize(int health)
    {
        maxHealth = Mathf.Max(1, health);
        currentHealth = maxHealth;
        SetupVisuals();
        SetupCollision();
        SetupHpBar();
        UpdateHpBar();
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        baseScale = Vector3.one * size;
        SetupVisuals();
        SetupCollision();
        SetupHpBar();
        UpdateHpBar();
    }

    private void Update()
    {
        if (isClearing) return;

        Vector3 movement = new Vector3(horizontalDirection * horizontalSpeed, -moveSpeed, 0f) * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (transform.position.x > xLimit)
        {
            horizontalDirection = -1f;
            transform.position = new Vector3(xLimit, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < -xLimit)
        {
            horizontalDirection = 1f;
            transform.position = new Vector3(-xLimit, transform.position.y, transform.position.z);
        }

        if (transform.position.y < -4.9f)
        {
            TriggerGameOver();
        }

        UpdateHpBarPosition();
        UpdateDamageFeedback();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isClearing) return;

        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            currentHealth -= Mathf.Max(1, bullet.attackPower);
            Destroy(collision.gameObject);
            PlayDamageFeedback();
            UpdateHpBar();

            if (currentHealth <= 0)
            {
                TriggerClear();
            }

            return;
        }

        if (collision.GetComponent<PlayerControllerStage00>() != null)
        {
            TriggerGameOver();
        }
    }

    private void TriggerClear()
    {
        if (isClearing) return;
        isClearing = true;

        Collider2D bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null) bossCollider.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        PlayerPrefs.SetInt("ClearScore", 0);
        PlayerPrefs.Save();

        StartCoroutine(PlayDefeatRoutine());
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
        bossRenderer = GetComponent<SpriteRenderer>();
        if (bossRenderer == null) bossRenderer = gameObject.AddComponent<SpriteRenderer>();
        bossRenderer.sprite = GetBossSprite();
        bossRenderer.color = Color.white;
        bossRenderer.sortingOrder = 17;
        transform.localScale = Vector3.one * size;
        baseScale = transform.localScale;

    }

    private void SetupHpBar()
    {
        if (hpFillRenderer != null) return;

        PlayerControllerStage00 player = FindObjectOfType<PlayerControllerStage00>();
        float playableXLimit = player != null ? player.xLimit : 2.1f;
        hpBarWidth = Mathf.Max(0.5f, playableXLimit * 2f - 0.25f);

        GameObject barRootObject = new GameObject("Stage00BossHpBar");
        hpBarRoot = barRootObject.transform;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(hpBarRoot, false);
        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = GetHpBarSprite();
        backgroundRenderer.color = new Color(0.06f, 0.04f, 0.04f, 0.92f);
        backgroundRenderer.sortingOrder = 28;
        backgroundObject.transform.localScale = new Vector3(hpBarWidth, hpBarHeight, 1f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(hpBarRoot, false);
        hpFillTransform = fillObject.transform;
        hpFillRenderer = fillObject.AddComponent<SpriteRenderer>();
        hpFillRenderer.sprite = GetHpBarSprite();
        hpFillRenderer.color = new Color(1f, 0.16f, 0.05f, 1f);
        hpFillRenderer.sortingOrder = 29;

        GameObject frameObject = new GameObject("Frame");
        frameObject.transform.SetParent(hpBarRoot, false);
        SpriteRenderer frameRenderer = frameObject.AddComponent<SpriteRenderer>();
        frameRenderer.sprite = GetHpBarSprite();
        frameRenderer.color = new Color(1f, 0.95f, 0.75f, 0.92f);
        frameRenderer.sortingOrder = 27;
        frameObject.transform.localScale = new Vector3(hpBarWidth + 0.08f, hpBarHeight + 0.08f, 1f);

        backgroundObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        fillObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        frameObject.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        UpdateHpBarPosition();
    }

    private void UpdateHpBar()
    {
        if (hpFillTransform == null) return;

        float fillRate = Mathf.Clamp01((float)currentHealth / maxHealth);
        hpFillTransform.localScale = new Vector3(hpBarWidth * fillRate, hpBarHeight, 1f);
        hpFillTransform.localPosition = new Vector3(-(hpBarWidth - hpBarWidth * fillRate) * 0.5f, 0f, -0.01f);
    }

    private void UpdateHpBarPosition()
    {
        if (hpBarRoot == null) return;

        float y = 4.65f;
        Camera camera = Camera.main;
        if (camera != null)
        {
            float zDistance = Mathf.Abs(camera.transform.position.z);
            Vector3 topCenter = camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height, zDistance));
            y = topCenter.y - hpBarTopMargin;
        }

        hpBarRoot.position = new Vector3(0f, y, -0.2f);
    }

    private void PlayDamageFeedback()
    {
        damageFeedbackTimer = 0.12f;
    }

    private void UpdateDamageFeedback()
    {
        if (damageFeedbackTimer <= 0f)
        {
            if (bossRenderer != null) bossRenderer.color = Color.white;
            if (hpFillRenderer != null) hpFillRenderer.color = new Color(1f, 0.16f, 0.05f, 1f);
            transform.localScale = baseScale;
            return;
        }

        damageFeedbackTimer -= Time.deltaTime;
        float flash = Mathf.Clamp01(damageFeedbackTimer / 0.12f);
        if (bossRenderer != null) bossRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.36f, 0.22f, 1f), flash);
        if (hpFillRenderer != null) hpFillRenderer.color = Color.Lerp(new Color(1f, 0.16f, 0.05f, 1f), new Color(1f, 0.95f, 0.18f, 1f), flash);
        transform.localScale = baseScale * (1f + 0.05f * flash);
    }

    private System.Collections.IEnumerator PlayDefeatRoutine()
    {
        ClearProjectilesForDefeat();
        Time.timeScale = defeatSlowTimeScale;
        if (hpBarRoot != null) hpBarRoot.gameObject.SetActive(false);

        Vector3 centerPosition = transform.position;
        SpawnDefeatCharge(centerPosition);

        float elapsed = 0f;
        bool didBlast = false;
        while (elapsed < defeatDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / defeatDuration);
            float chargeProgress = Mathf.Clamp01(progress / 0.28f);
            float blastProgress = Mathf.Clamp01((progress - 0.28f) / 0.72f);
            float shake = progress < 0.28f
                ? Mathf.Lerp(0.05f, 0.34f, chargeProgress)
                : Mathf.Lerp(0.34f, 0.02f, blastProgress);
            float pulse = progress < 0.28f
                ? Mathf.Lerp(1f, 0.72f, chargeProgress) + Mathf.Sin(progress * Mathf.PI * 34f) * 0.08f
                : Mathf.Lerp(1.45f, 2.65f, blastProgress);

            transform.position = centerPosition + new Vector3(Random.Range(-shake, shake), Random.Range(-shake, shake), 0f);
            transform.localScale = baseScale * pulse;
            transform.Rotate(0f, 0f, Mathf.Lerp(120f, 720f, progress) * Time.unscaledDeltaTime);

            if (!didBlast && progress >= 0.28f)
            {
                didBlast = true;
                SpawnDefeatBlast(centerPosition);
                SpawnDefeatFragments(centerPosition);
            }

            if (bossRenderer != null)
            {
                Color hotColor = progress < 0.28f
                    ? Color.Lerp(Color.white, new Color(1f, 0.04f, 0.01f, 1f), Mathf.PingPong(progress * 24f, 1f))
                    : Color.Lerp(new Color(1f, 0.5f, 0.08f, 1f), Color.white, Mathf.PingPong(progress * 10f, 1f));
                hotColor.a = progress < 0.28f ? 1f : Mathf.Lerp(0.9f, 0f, Mathf.Clamp01(blastProgress * 1.4f));
                bossRenderer.color = hotColor;
            }

            yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(ClearSceneName, LoadSceneMode.Single);
    }

    private void SpawnDefeatCharge(Vector3 centerPosition)
    {
        CreateShockwave(centerPosition, 0.8f, 1.9f, 0.9f, new Color(1f, 0.04f, 0.02f, 0.75f), 34);
        CreateSpriteFlash(centerPosition, 0.55f, 1.25f, 0.9f, new Color(1f, 0.08f, 0.02f, 0.65f), 33);
        CreateParticleBurst(
            "Stage00BossChargeSparks",
            centerPosition,
            38,
            0.75f,
            new Vector2(0.45f, 1.2f),
            new Vector2(0.04f, 0.1f),
            new Color(1f, 0.16f, 0.02f, 1f),
            new Color(1f, 0.82f, 0.12f, 0.7f),
            35,
            0.25f);
    }

    private void SpawnDefeatBlast(Vector3 centerPosition)
    {
        CreateSpriteFlash(centerPosition, 0.75f, 5.7f, 0.6f, new Color(1f, 0.88f, 0.35f, 0.9f), 37);
        CreateShockwave(centerPosition, 0.9f, 7.4f, 1.25f, new Color(1f, 0.72f, 0.18f, 0.95f), 38);
        CreateShockwave(centerPosition, 0.6f, 5.2f, 0.92f, new Color(1f, 0.12f, 0.02f, 0.8f), 39);
        CreateShockwave(centerPosition, 1.4f, 8.8f, 1.55f, new Color(0.25f, 0.9f, 1f, 0.48f), 36);
        CreateParticleBurst(
            "Stage00BossCoreBurst",
            centerPosition,
            160,
            1.55f,
            new Vector2(2.3f, 7.4f),
            new Vector2(0.07f, 0.24f),
            new Color(1f, 0.25f, 0.02f, 1f),
            new Color(1f, 0.95f, 0.38f, 0.85f),
            40,
            0.45f);
        CreateParticleBurst(
            "Stage00BossEmberTrail",
            centerPosition,
            70,
            1.9f,
            new Vector2(0.9f, 3.2f),
            new Vector2(0.12f, 0.34f),
            new Color(0.72f, 0.18f, 0.06f, 1f),
            new Color(0.22f, 0.08f, 0.04f, 0.85f),
            32,
            0.8f);
    }

    private void ClearProjectilesForDefeat()
    {
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null) Destroy(bullet.gameObject);
        }

        EarthBullet[] earthBullets = FindObjectsOfType<EarthBullet>();
        foreach (EarthBullet earthBullet in earthBullets)
        {
            if (earthBullet != null) Destroy(earthBullet.gameObject);
        }

        EnemyBullet[] enemyBullets = FindObjectsOfType<EnemyBullet>();
        foreach (EnemyBullet enemyBullet in enemyBullets)
        {
            if (enemyBullet != null) Destroy(enemyBullet.gameObject);
        }
    }

    private void SpawnDefeatFragments(Vector3 centerPosition)
    {
        for (int i = 0; i < 58; i++)
        {
            GameObject fragment = new GameObject("Stage00BossFragment");
            fragment.transform.position = centerPosition + (Vector3)(Random.insideUnitCircle * 0.75f);
            fragment.transform.localScale = new Vector3(Random.Range(0.08f, 0.34f), Random.Range(0.08f, 0.24f), 1f);
            fragment.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            SpriteRenderer fragmentRenderer = fragment.AddComponent<SpriteRenderer>();
            fragmentRenderer.sprite = GetHpBarSprite();
            fragmentRenderer.color = Random.value < 0.5f
                ? new Color(1f, 0.23f, 0.03f, 0.95f)
                : new Color(0.52f, 0.34f, 0.2f, 0.95f);
            fragmentRenderer.sortingOrder = 31;

            Stage00BossFragment fragmentMotion = fragment.AddComponent<Stage00BossFragment>();
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.1f) direction = Vector2.up;
            fragmentMotion.Initialize(direction * Random.Range(3.2f, 8.8f), Random.Range(260f, 920f), Random.Range(1.25f, 2.25f));
        }

        for (int i = 0; i < 18; i++)
        {
            GameObject flare = new GameObject("Stage00BossFlare");
            flare.transform.position = centerPosition + (Vector3)(Random.insideUnitCircle * 0.22f);
            flare.transform.localScale = new Vector3(Random.Range(0.08f, 0.16f), Random.Range(1.0f, 2.2f), 1f);
            flare.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            SpriteRenderer flareRenderer = flare.AddComponent<SpriteRenderer>();
            flareRenderer.sprite = GetHpBarSprite();
            flareRenderer.color = new Color(1f, 0.45f, 0.04f, 0.82f);
            flareRenderer.sortingOrder = 32;

            Stage00BossFragment flareMotion = flare.AddComponent<Stage00BossFragment>();
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.1f) direction = Vector2.up;
            flareMotion.Initialize(direction * Random.Range(1.6f, 4.4f), Random.Range(90f, 240f), Random.Range(1.4f, 2.4f));
        }
    }

    private void CreateParticleBurst(
        string effectName,
        Vector3 position,
        short count,
        float duration,
        Vector2 speedRange,
        Vector2 sizeRange,
        Color startColor,
        Color endColor,
        int sortingOrder,
        float radius)
    {
        GameObject effectObject = new GameObject(effectName);
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = duration;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(duration * 0.45f, duration);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedRange.x, speedRange.y);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        main.startColor = new ParticleSystem.MinMaxGradient(startColor, endColor);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.useUnscaledTime = true;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;
        shape.arc = 360f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.radial = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(endColor, 0.55f),
                new GradientColorKey(new Color(0.12f, 0.03f, 0.02f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.85f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = gradient;

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null) particleRenderer.material = new Material(spriteShader);
        particleRenderer.sortingOrder = sortingOrder;

        particles.Play();
        effectObject.AddComponent<Stage00BossTimedDestroy>().Initialize(duration + 0.25f);
    }

    private void CreateShockwave(Vector3 position, float startScale, float endScale, float duration, Color color, int sortingOrder)
    {
        GameObject shockwave = new GameObject("Stage00BossShockwave");
        shockwave.transform.position = position;
        shockwave.transform.localScale = Vector3.one * startScale;

        SpriteRenderer shockwaveRenderer = shockwave.AddComponent<SpriteRenderer>();
        shockwaveRenderer.sprite = GetShockwaveSprite();
        shockwaveRenderer.color = color;
        shockwaveRenderer.sortingOrder = sortingOrder;

        shockwave.AddComponent<Stage00BossShockwave>().Initialize(startScale, endScale, duration);
    }

    private void CreateSpriteFlash(Vector3 position, float startScale, float endScale, float duration, Color color, int sortingOrder)
    {
        GameObject flash = new GameObject("Stage00BossFlash");
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * startScale;

        SpriteRenderer flashRenderer = flash.AddComponent<SpriteRenderer>();
        flashRenderer.sprite = GetGlowSprite();
        flashRenderer.color = color;
        flashRenderer.sortingOrder = sortingOrder;

        flash.AddComponent<Stage00BossShockwave>().Initialize(startScale, endScale, duration);
    }

    private void OnDestroy()
    {
        if (hpBarRoot != null) Destroy(hpBarRoot.gameObject);
    }

    private static Sprite GetBossSprite()
    {
        if (bossSprite != null) return bossSprite;

        bossSprite = Resources.Load<Sprite>(BossSpriteResourcePath);
        if (bossSprite != null) return bossSprite;

        const int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = Vector2.one * (textureSize - 1) * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                float angle = Mathf.Atan2(point.y - center.y, point.x - center.x);
                float radius = 50f;
                float auraRadius = radius + 8f;

                if (distance > auraRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                if (distance > radius)
                {
                    float aura = Mathf.InverseLerp(auraRadius, radius, distance);
                    texture.SetPixel(x, y, Color.Lerp(new Color(1f, 0.05f, 0f, 0.45f), new Color(1f, 0.35f, 0f, 0.95f), aura));
                    continue;
                }

                int cellX = x / 6;
                int cellY = y / 6;
                float facetNoise = Mathf.PerlinNoise(cellX * 0.61f, cellY * 0.73f);
                float light = Mathf.Clamp01(0.35f + (center.y - y) * 0.008f + (center.x - x) * 0.004f);
                float shade = Mathf.InverseLerp(radius, 0f, distance);
                float facet = Mathf.Clamp01(facetNoise * 0.7f + light * 0.55f + shade * 0.25f);

                Color darkRock = new Color(0.11f, 0.09f, 0.08f);
                Color midRock = new Color(0.48f, 0.38f, 0.28f);
                Color lightRock = new Color(0.82f, 0.67f, 0.47f);
                Color color = facet < 0.45f
                    ? Color.Lerp(darkRock, midRock, facet / 0.45f)
                    : Color.Lerp(midRock, lightRock, (facet - 0.45f) / 0.55f);
                color.a = 1f;

                float coreDistance = Vector2.Distance(point, center);
                if (coreDistance < 16f)
                {
                    float core = Mathf.InverseLerp(16f, 0f, coreDistance);
                    Color coreColor = Color.Lerp(new Color(0.35f, 0f, 0f), new Color(1f, 0.05f, 0.02f), core);
                    color = Color.Lerp(color, coreColor, Mathf.Clamp01(core * 1.25f));
                }

                float crackA = Mathf.Abs(point.y - center.y - Mathf.Sin((point.x - center.x) * 0.13f) * 7f);
                float crackB = Mathf.Abs(point.x - center.x - Mathf.Sin((point.y - center.y) * 0.11f) * 8f);
                bool isFacetEdge = x % 12 == 0 || y % 12 == 0;
                bool isDarkCrack = isFacetEdge && facetNoise < 0.42f && distance < radius - 3f;
                bool isLavaCrack = (crackA < 1.4f && Mathf.Abs(point.x - center.x) < 39f) ||
                                   (crackB < 1.2f && Mathf.Abs(point.y - center.y) < 34f);
                if (isDarkCrack)
                {
                    color = Color.Lerp(color, new Color(0.02f, 0.018f, 0.02f), 0.82f);
                }

                if (isLavaCrack && coreDistance > 11f)
                {
                    float crackGlow = Mathf.Clamp01(1f - Mathf.Min(crackA, crackB) / 1.6f);
                    color = Color.Lerp(color, new Color(1f, 0.35f, 0.02f), crackGlow * 0.95f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Point;
        bossSprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, textureSize);
        return bossSprite;
    }

    private static Sprite GetHpBarSprite()
    {
        if (hpBarSprite != null) return hpBarSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        hpBarSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        return hpBarSprite;
    }

    private static Sprite GetShockwaveSprite()
    {
        if (shockwaveSprite != null) return shockwaveSprite;

        const int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = Vector2.one * (textureSize - 1) * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float ring = Mathf.Abs(distance - 45f);
                float alpha = Mathf.Clamp01(1f - ring / 8f);
                alpha *= Mathf.Clamp01((60f - distance) / 8f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        shockwaveSprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, textureSize);
        return shockwaveSprite;
    }

    private static Sprite GetGlowSprite()
    {
        if (glowSprite != null) return glowSprite;

        const int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = Vector2.one * (textureSize - 1) * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - distance / 58f);
                alpha = alpha * alpha;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        glowSprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, textureSize);
        return glowSprite;
    }

    private class Stage00BossFragment : MonoBehaviour
    {
        private Vector2 velocity;
        private float rotationSpeed;
        private float lifeTime;
        private float age;
        private SpriteRenderer spriteRenderer;

        public void Initialize(Vector2 initialVelocity, float initialRotationSpeed, float duration)
        {
            velocity = initialVelocity;
            rotationSpeed = initialRotationSpeed;
            lifeTime = duration;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            age += Time.unscaledDeltaTime;
            transform.position += (Vector3)(velocity * Time.unscaledDeltaTime);
            transform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
            velocity *= Mathf.Pow(0.35f, Time.unscaledDeltaTime);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(age / lifeTime));
                spriteRenderer.color = color;
            }

            if (age >= lifeTime) Destroy(gameObject);
        }
    }

    private class Stage00BossShockwave : MonoBehaviour
    {
        private float startScale;
        private float endScale;
        private float lifeTime;
        private float age;
        private SpriteRenderer spriteRenderer;
        private Color startColor;

        public void Initialize(float initialScale, float targetScale, float duration)
        {
            startScale = initialScale;
            endScale = targetScale;
            lifeTime = duration;
            spriteRenderer = GetComponent<SpriteRenderer>();
            startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        }

        private void Update()
        {
            age += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(age / lifeTime);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);

            if (spriteRenderer != null)
            {
                Color color = startColor;
                color.a = startColor.a * Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = color;
            }

            if (age >= lifeTime) Destroy(gameObject);
        }
    }

    private class Stage00BossTimedDestroy : MonoBehaviour
    {
        private float lifeTime;
        private float age;

        public void Initialize(float duration)
        {
            lifeTime = duration;
        }

        private void Update()
        {
            age += Time.unscaledDeltaTime;
            if (age >= lifeTime) Destroy(gameObject);
        }
    }

}
