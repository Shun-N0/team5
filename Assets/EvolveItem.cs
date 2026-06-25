using UnityEngine;

public class EvolveItem : MonoBehaviour
{
    public float fallSpeed = 2.0f; // ★落ちるスピード（調整可能）
    public float lifeTime = 10.0f; // 画面内に残る最大時間

    void Start()
    {
        // 念のため、時間が経ちすぎたら消えるようにしておく
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ★追加：毎フレーム、下に移動させる
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 画面の下端（地球より下）に行ったら消す
        if (transform.position.y < -6.0f)
        {
            Destroy(gameObject);
        }
    }
}