using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int attackPower = 1;

    void Update()
    {
        // ★修正ポイント：Vector2.up（世界の上）ではなく transform.up（弾の正面）に進む
        // これにより、PlayerControllerで回転させて発射した斜めの弾が、正しく斜めに進みます
        transform.position += transform.up * speed * Time.deltaTime;

        // 画面の外に出たら自動で消える（上下左右の端を判定）
        // 縦長画面に合わせて横(x)の判定も追加しています
        if (transform.position.y > 5.5f || transform.position.y < -5.5f || 
            transform.position.x > 3.0f || transform.position.x < -3.0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 敵の弾（EnemyBullet）またはスタン弾（StunBullet）に当たったら、自分も相手も消える（撃ち落とし）
        if (collision.CompareTag("EnemyBullet") || collision.CompareTag("StunBullet"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    // ボスや敵から弾のスピードを変更できるようにするための関数（予備）
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}