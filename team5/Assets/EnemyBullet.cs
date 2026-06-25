using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f; // 下向きの速度

    // Enemy.cs から SendMessage で呼ばれる：弾速を上書きする
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        // 下方向に移動
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        // 画面外（下端）に行ったら消す
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
}