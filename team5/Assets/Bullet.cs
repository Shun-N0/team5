using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int attackPower = 1;

    void Update()
    {
        // 上に飛んでいく
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        // 画面の外（上側）に出たら自動で消えるようにする
        if (transform.position.y > 4.8f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyBullet")) return;

        Destroy(collision.gameObject);
        Destroy(gameObject);
    }
}
