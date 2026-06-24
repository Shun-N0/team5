using UnityEngine;

public class GoalLine : MonoBehaviour
{
    [Tooltip("BackgroundScroller の scrollSpeed と同じ値にしてください")]
    public float scrollSpeed = 3f;

    [Tooltip("ゲーム開始時の画面上端から何ユニット先にゴールを置くか")]
    public float stageLength = 50f;

    void Start()
    {
        Camera cam = Camera.main;
        float screenWidth = cam.orthographicSize * 2f * cam.aspect;
        float startY = cam.transform.position.y + cam.orthographicSize + stageLength;

        transform.position = new Vector3(cam.transform.position.x, startY, 0f);

        // ゴール帯ビジュアル（黄色）
        var sr = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = MakeSprite(new Color(1f, 0.85f, 0f));
        sr.sortingOrder = 5;
        transform.localScale = new Vector3(screenWidth * 1.2f, 0.5f, 1f);

        // トリガー判定
        var col = GetComponent<BoxCollider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one; // スケール込みで screenWidth x 0.5 になる
    }

    void Update()
    {
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StageManager.Instance?.TriggerClear();
    }

    Sprite MakeSprite(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
    }
}
