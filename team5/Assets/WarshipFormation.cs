using System.Collections.Generic;
using UnityEngine;

public class WarshipFormation : MonoBehaviour
{
    public enum FormationType
    {
        FiveShips,
        ThreeShips
    }

    [Header("軍艦設定")]
    [SerializeField] private Sprite warshipSprite;
    [SerializeField] private Color unitColor = Color.white;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip killSound;
    [SerializeField] private int unitHealth = 2;
    [SerializeField] private int unitScore = 250;
    [SerializeField] private float unitScale = 0.55f;

    [Header("編隊設定")]
    [SerializeField] private FormationType formationType = FormationType.FiveShips;

    [Header("編隊移動設定")]
    [SerializeField] private float horizontalSpeed = 1.2f;
    [SerializeField] private float descentSpeed = 0.75f;
    [SerializeField] private float xLimit = 1.25f;
    [SerializeField] private float spawnYOffset = 1.4f;

    [Header("一斉射撃設定")]
    [SerializeField] private float synchronizedShotInterval = 1.8f;
    [SerializeField] private float bulletSpeed = 2.4f;
    [SerializeField] private float bulletScale = 0.45f;

    private readonly List<WarshipUnit> units = new List<WarshipUnit>();
    private float direction = 1f;
    private float shotTimer;

    private static readonly Vector2[] FiveShipOffsets =
    {
        new Vector2(-1.4f, 0f),
        new Vector2(-0.7f, -0.45f),
        new Vector2(0f, -0.8f),
        new Vector2(0.7f, -0.45f),
        new Vector2(1.4f, 0f)
    };

    private static readonly Vector2[] ThreeShipOffsets =
    {
        new Vector2(-0.8f, 0f),
        new Vector2(0f, -0.55f),
        new Vector2(0.8f, 0f)
    };

    public FormationType Type => formationType;

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float startY = mainCamera.transform.position.y + mainCamera.orthographicSize + spawnYOffset;
            transform.position = new Vector3(0f, startY, 0f);
        }

        CreateUnits();
        shotTimer = synchronizedShotInterval * 0.6f;
    }

    private void Update()
    {
        MoveFormation();
        ShootTogether();

        if (transform.position.y < -6.5f)
        {
            Destroy(gameObject);
        }
    }

    private void CreateUnits()
    {
        Vector2[] formationOffsets = formationType == FormationType.ThreeShips
            ? ThreeShipOffsets
            : FiveShipOffsets;

        foreach (Vector2 offset in formationOffsets)
        {
            GameObject unitObject = new GameObject("Warship");
            unitObject.tag = "Enemy";
            unitObject.transform.SetParent(transform);
            unitObject.transform.localPosition = offset;
            unitObject.transform.localScale = Vector3.one * unitScale;

            SpriteRenderer spriteRenderer = unitObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = warshipSprite;
            spriteRenderer.color = unitColor;
            spriteRenderer.sortingOrder = 1;

            BoxCollider2D collider = unitObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.72f, 0.9f);

            WarshipUnit unit = unitObject.AddComponent<WarshipUnit>();
            unit.Initialize(this, unitHealth);
            units.Add(unit);
        }
    }

    private void MoveFormation()
    {
        Vector3 movement = new Vector3(
            direction * horizontalSpeed,
            -descentSpeed,
            0f
        ) * Time.deltaTime;

        transform.Translate(movement);

        if (transform.position.x >= xLimit)
        {
            direction = -1f;
        }
        else if (transform.position.x <= -xLimit)
        {
            direction = 1f;
        }
    }

    private void ShootTogether()
    {
        shotTimer -= Time.deltaTime;
        if (shotTimer > 0f) return;

        foreach (WarshipUnit unit in units)
        {
            if (unit != null)
            {
                unit.Fire(enemyBulletPrefab, bulletSpeed, bulletScale);
            }
        }

        shotTimer = synchronizedShotInterval;
    }

    public void OnUnitDestroyed(WarshipUnit destroyedUnit)
    {
        units.Remove(destroyedUnit);

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, destroyedUnit.transform.position, Quaternion.identity);
        }

        if (killSound != null && StageManager.Instance != null)
        {
            StageManager.Instance.PlaySE(killSound, 0.15f);
        }

        StageManager.Instance?.AddScore(unitScore);

        if (units.Count == 0)
        {
            Destroy(gameObject);
        }
    }
}
