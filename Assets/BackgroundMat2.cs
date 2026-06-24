using UnityEngine;

public class BackgroundMat2 : MonoBehaviour
{
    // スクロールするスピード（Inspectorから調整可能）
    public float scrollSpeed = 0.1f; 
    
    private Renderer bgRenderer;

    void Start()
    {
        // 自分のコンポーネントからRendererを取得
        bgRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // 時間経過に合わせてY軸のオフセット（ずれ）を計算
        float offset = Time.time * scrollSpeed;
        
        // マテリアルのメインテクスチャのオフセットを更新
        bgRenderer.material.mainTextureOffset = new Vector2(0, offset);
    }
}