using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f; // 移動速度
    private Vector2 moveDirection = Vector2.down; // 進行方向（デフォルトは真下＝従来通り）

    // Enemy.cs から SendMessage で呼ばれる：弾速を上書きする
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // ★追加：Enemy.cs から SendMessage で呼ばれる：進行方向を上書きする
    // （拡散弾・狙い撃ちで使用。呼ばれなければ真下のまま）
    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }

    void Update()
    {
        // 指定された方向へ移動
        transform.Translate(moveDirection * speed * Time.deltaTime);

        // 画面外に出たら消す（斜め弾も考慮して上下左右をチェック）
        if (transform.position.y < -6f || transform.position.y > 6f
            || Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }
}