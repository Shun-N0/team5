using UnityEngine;

public class GoalLine : MonoBehaviour
{
    [Tooltip("降りてくるスピード")]
    public float scrollSpeed = 3f;

    [Tooltip("出現したとき、画面上端からどれくらい上に置くか（1〜2がおすすめ）")]
    public float appearOffset = 1.0f;

    void Start()
    {
        // --- 出現時の位置合わせ ---
        Camera cam = Camera.main;
        float screenWidth = cam.orthographicSize * 2f * cam.aspect;
        
        // ★修正：遠く（50）ではなく、画面のすぐ上（appearOffset）にセットする
        float startY = cam.transform.position.y + cam.orthographicSize + appearOffset;
        transform.position = new Vector3(cam.transform.position.x, startY, 0f);

        // ビジュアル設定
        var sr = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        // もし画像がNoneなら、ここで白い四角を生成して色を塗る
        if (sr.sprite == null) {
            sr.sprite = MakeSprite(Color.white);
        }
        sr.color = new Color(1f, 0.85f, 0f); // 金色
        sr.sortingOrder = 10; // 一番手前に表示
        transform.localScale = new Vector3(screenWidth * 1.2f, 0.5f, 1f);

        // 当たり判定の設定
        var col = GetComponent<BoxCollider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        // 常に下に降りてくる
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーに触れたらクリア！
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゴールに到達！");
            StageManager.Instance?.TriggerClear();
        }
    }

    Sprite MakeSprite(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
    }
}