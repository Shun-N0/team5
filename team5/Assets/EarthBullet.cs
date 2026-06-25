using UnityEngine;

public class EarthBullet : MonoBehaviour
{
    private static readonly Color EarthBulletColor = new Color(0.1f, 0.9f, 1f);
    private const float SizeMultiplier = 1.7f;

    private void Awake()
    {
        gameObject.name = "EarthBullet";
        transform.localScale *= SizeMultiplier;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        spriteRenderer.color = EarthBulletColor;
        spriteRenderer.sortingOrder = 2;
    }
}
