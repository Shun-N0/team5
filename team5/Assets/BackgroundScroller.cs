using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 3f;

    private GameObject[] panels = new GameObject[2];
    private float panelHeight;

    void Start()
    {
        Camera cam = Camera.main;
        panelHeight = cam.orthographicSize * 2f;
        float panelWidth = panelHeight * cam.aspect;

        Color colorA = new Color(0.05f, 0.05f, 0.25f); // 濃い青
        Color colorB = new Color(0.15f, 0.15f, 0.45f); // やや明るい青
        int stripeCount = 8;
        float stripeH = panelHeight / stripeCount;

        Sprite sprA = MakeSprite(colorA);
        Sprite sprB = MakeSprite(colorB);

        for (int i = 0; i < 2; i++)
        {
            panels[i] = new GameObject("BG_" + i);
            panels[i].transform.SetParent(transform);
            panels[i].transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y + panelHeight * i,
                0f
            );

            for (int s = 0; s < stripeCount; s++)
            {
                var stripe = new GameObject("stripe_" + s);
                stripe.transform.SetParent(panels[i].transform);
                var sr = stripe.AddComponent<SpriteRenderer>();
                sr.sprite = (s % 2 == 0) ? sprA : sprB;
                sr.sortingOrder = -10;
                stripe.transform.localScale = new Vector3(panelWidth, stripeH, 1f);
                float stripeY = (-panelHeight / 2f) + (s + 0.5f) * stripeH;
                stripe.transform.localPosition = new Vector3(0f, stripeY, 0f);
            }
        }
    }

    void Update()
    {
        Vector3 delta = Vector3.down * scrollSpeed * Time.deltaTime;
        Camera cam = Camera.main;
        float recycleThreshold = cam.transform.position.y - cam.orthographicSize - panelHeight / 2f;

        foreach (var p in panels)
        {
            p.transform.Translate(delta);

            if (p.transform.position.y < recycleThreshold)
            {
                float maxY = float.NegativeInfinity;
                foreach (var q in panels)
                    if (q != p) maxY = Mathf.Max(maxY, q.transform.position.y);
                p.transform.position = new Vector3(p.transform.position.x, maxY + panelHeight, 0f);
            }
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
