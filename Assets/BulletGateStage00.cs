using TMPro;
using UnityEngine;

public class BulletGateStage00 : MonoBehaviour
{
    public enum GateOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    private const float DestroyY = -5.8f;
    private static Sprite gateSprite;

    [SerializeField] private int bulletChange = 1;
    [SerializeField] private GateOperation operation = GateOperation.Add;
    [SerializeField] private float fallSpeed = 2.4f;
    [SerializeField] private Vector2 gateSize = new Vector2(2.1f, 0.8f);
    private bool wasUsed;

    public void Initialize(GateOperation gateOperation, int value, float speed, Vector2 size)
    {
        operation = gateOperation;
        bulletChange = Mathf.Max(1, value);
        fallSpeed = speed;
        gateSize = size;
        SetupCollision();
        SetupVisuals();
    }

    private void Awake()
    {
        SetupCollision();
        SetupVisuals();
    }

    private void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
        if (transform.position.y < DestroyY) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (wasUsed) return;

        PlayerControllerStage00 player = collision.GetComponent<PlayerControllerStage00>();
        if (player == null) return;
        if (!IsPlayerInsideThisGate(player.transform.position)) return;

        wasUsed = true;
        ApplyGate(player);

        Destroy(gameObject);
    }

    private void ApplyGate(PlayerControllerStage00 player)
    {
        switch (operation)
        {
            case GateOperation.Add:
                player.AddBullets(bulletChange);
                break;
            case GateOperation.Subtract:
                player.RemoveBullets(bulletChange);
                break;
            case GateOperation.Multiply:
                player.MultiplyBullets(bulletChange);
                break;
            case GateOperation.Divide:
                player.DivideBullets(bulletChange);
                break;
        }
    }

    private bool IsPlayerInsideThisGate(Vector3 playerPosition)
    {
        float halfWidth = gateSize.x * 0.5f;
        float leftEdge = transform.position.x - halfWidth;
        float rightEdge = transform.position.x + halfWidth;
        bool isLeftGate = transform.position.x < 0f;

        if (isLeftGate && playerPosition.x >= 0f) return false;
        if (!isLeftGate && playerPosition.x < 0f) return false;

        return playerPosition.x >= leftEdge && playerPosition.x <= rightEdge;
    }

    private void SetupCollision()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = gateSize;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void SetupVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetGateSprite();
        sr.color = IsPositiveGate() ? new Color(0.15f, 0.8f, 0.35f, 0.45f) : new Color(0.9f, 0.25f, 0.25f, 0.45f);
        sr.sortingOrder = 20;
        transform.localScale = new Vector3(gateSize.x, gateSize.y, 1f);

        TextMeshPro label = GetComponentInChildren<TextMeshPro>();
        if (label == null)
        {
            GameObject labelObject = new GameObject("GateLabel");
            labelObject.transform.SetParent(transform, false);
            label = labelObject.AddComponent<TextMeshPro>();
        }

        label.text = GetGateLabel();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 5f;
        label.color = Color.white;
        label.transform.localPosition = new Vector3(0f, -0.22f, -0.1f);
        label.transform.localScale = Vector3.one;

        MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
        if (labelRenderer != null) labelRenderer.sortingOrder = 21;

        SetupGateFrame();
    }

    private string GetGateLabel()
    {
        switch (operation)
        {
            case GateOperation.Add:
                return "+" + bulletChange;
            case GateOperation.Subtract:
                return "-" + bulletChange;
            case GateOperation.Multiply:
                return "x" + bulletChange;
            case GateOperation.Divide:
                return "÷" + bulletChange;
            default:
                return bulletChange.ToString();
        }
    }

    private bool IsPositiveGate()
    {
        return operation == GateOperation.Add || operation == GateOperation.Multiply;
    }

    private void SetupGateFrame()
    {
        Color postColor = IsPositiveGate()
            ? new Color(0.75f, 1f, 0.82f)
            : new Color(1f, 0.78f, 0.72f);

        CreateFramePart("LeftPost", new Vector3(-0.5f, 0f, -0.05f), new Vector2(0.08f / gateSize.x, 1.15f), postColor, 22);
        CreateFramePart("RightPost", new Vector3(0.5f, 0f, -0.05f), new Vector2(0.08f / gateSize.x, 1.15f), postColor, 22);
    }

    private void CreateFramePart(string partName, Vector3 localPosition, Vector2 localScale, Color color, int sortingOrder)
    {
        Transform existing = transform.Find(partName);
        GameObject partObject = existing != null ? existing.gameObject : new GameObject(partName);
        partObject.transform.SetParent(transform, false);
        partObject.transform.localPosition = localPosition;
        partObject.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);

        SpriteRenderer sr = partObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = partObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetGateSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
    }

    private static Sprite GetGateSprite()
    {
        if (gateSprite != null) return gateSprite;

        Texture2D texture = new Texture2D(16, 16);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Point;
        gateSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f, 16);
        return gateSprite;
    }
}
