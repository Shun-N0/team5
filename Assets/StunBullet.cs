using UnityEngine;

public class StunBullet : MonoBehaviour
{
    public float speed = 5f;
    public float stunDuration = 2.0f;
    private Vector2 moveDirection = Vector2.down; // 進行方向（デフォルトは真下＝従来通り）

    // ★追加：Enemy.cs から SendMessage で呼ばれる：進行方向を上書きする
    // （3方向スタン弾などで使用。呼ばれなければ真下のまま。弾速は既存挙動維持のため上書きしない）
    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
        if (transform.position.y < -6f || transform.position.y > 6f
            || Mathf.Abs(transform.position.x) > 12f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null) player.GetStunned(stunDuration);
            Destroy(gameObject);
        }
        // ★ここを Earth ではなく EarthController に修正
        else if (collision.CompareTag("Earth"))
        {
            EarthController earth = collision.GetComponent<EarthController>();
            if (earth != null) earth.GetStunned(stunDuration);
            Destroy(gameObject);
        }
    }
    
    public void SetSpeed(float newSpeed) { speed = newSpeed; }
}