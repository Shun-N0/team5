using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stage00WarningBlink : MonoBehaviour
{
    private TextMeshProUGUI warningText;
    private Image warningBackground;

    public void Initialize(TextMeshProUGUI text, Image background)
    {
        warningText = text;
        warningBackground = background;
    }

    private void Update()
    {
        if (warningText == null) return;

        float alpha = Mathf.Lerp(0.35f, 1f, Mathf.PingPong(Time.time * 4f, 1f));
        Color color = warningText.color;
        color.a = alpha;
        warningText.color = color;

        if (warningBackground != null)
        {
            Color backgroundColor = warningBackground.color;
            backgroundColor.a = Mathf.Lerp(0.12f, 0.34f, Mathf.PingPong(Time.time * 4f, 1f));
            warningBackground.color = backgroundColor;
        }

        float scale = Mathf.Lerp(0.96f, 1.04f, Mathf.PingPong(Time.time * 3f, 1f));
        transform.localScale = Vector3.one * scale;
    }
}
