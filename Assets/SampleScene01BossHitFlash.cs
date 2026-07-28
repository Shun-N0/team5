using System.Collections;
using UnityEngine;

public class SampleScene01BossHitFlash : MonoBehaviour
{
    [Header("被弾フラッシュ")]
    [SerializeField] private Color flashColor = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private float flashDuration = 0.12f;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    public void Play()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        RestoreColor();
        flashCoroutine = null;
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = color;
        }
    }

    private void RestoreColor()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = originalColors[i];
        }
    }
}
