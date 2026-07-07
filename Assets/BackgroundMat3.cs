using UnityEngine;

public class BackgroundMat3 : MonoBehaviour
{
    public float scrollSpeed = 0.05f; 
    private Material mat;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;
        }
    }

    void Update()
    {
        if (mat != null)
        {
            float offset = Time.time * scrollSpeed;
            
            // ★修正ポイント：箱があるかどうかを確認してから命令を送る
            // これで「_BaseMapがない！」というエラーを回避できます
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTextureOffset("_BaseMap", new Vector2(0, offset));
            }
            else if (mat.HasProperty("_MainTex"))
            {
                mat.SetTextureOffset("_MainTex", new Vector2(0, offset));
            }
        }
    }
}