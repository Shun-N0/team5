using UnityEngine;

public class EarthController : MonoBehaviour
{
    [Header("見た目設定")]
    [SerializeField] private Sprite earthSprite;

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float xLimit = 2.1f;
    [SerializeField] private float directionChangeInterval = 1.5f;

    [Header("発射設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shotInterval = 1.2f;
    [SerializeField] private float bulletSpawnOffset = 0.65f;

    private float moveDirection = 1f;
    private float directionTimer;
    private float shotTimer;

    private void Awake()
    {
        SetupAppearance();
        SetupCollision();
    }

    private void Start()
    {
        ChooseRandomDirection();
        directionTimer = directionChangeInterval;
        shotTimer = shotInterval;
    }

    private void Update()
    {
        Move();
        ShootAutomatically();
    }

    private void Move()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            ChooseRandomDirection();
            directionTimer = directionChangeInterval;
        }

        transform.Translate(Vector2.right * moveDirection * moveSpeed * Time.deltaTime);

        float clampedX = Mathf.Clamp(transform.position.x, -xLimit, xLimit);
        if (!Mathf.Approximately(clampedX, transform.position.x))
        {
            moveDirection *= -1f;
        }

        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    private void ShootAutomatically()
    {
        shotTimer -= Time.deltaTime;
        if (shotTimer > 0f || bulletPrefab == null) return;

        Vector3 spawnPosition = transform.position + Vector3.up * bulletSpawnOffset;
        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bulletObject.AddComponent<EarthBullet>();
        shotTimer = shotInterval;
    }

    private void ChooseRandomDirection()
    {
        moveDirection = Random.value < 0.5f ? -1f : 1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyBullet") && !collision.CompareTag("Enemy")) return;

        Destroy(collision.gameObject);
        StageManager.Instance?.TriggerGameOver();
    }

    private void SetupAppearance()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = earthSprite != null ? earthSprite : CreateCircleSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 5;
    }

    private void SetupCollision()
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.isTrigger = true;

        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        }
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.gravityScale = 0f;
    }

    private Sprite CreateCircleSprite()
    {
        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Vector2 center = Vector2.one * (textureSize - 1) / 2f;
        float radius = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(
            texture,
            new Rect(0, 0, textureSize, textureSize),
            Vector2.one * 0.5f,
            textureSize
        );
    }
}
