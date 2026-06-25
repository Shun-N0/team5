using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    public float speed = 5f;
    private Transform player;
    private Vector3 moveDirection = Vector3.down;
    private bool hasShifted = false;
    public float shiftDistance = 3f;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // ★追加：敵からスピードを受け取るための関数
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        if (player != null && !hasShifted)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < shiftDistance)
            {
                moveDirection = (player.position - transform.position).normalized;
                hasShifted = true;
            }
        }
        transform.position += moveDirection * speed * Time.deltaTime;
        if (transform.position.y < -6f || transform.position.y > 6f) Destroy(gameObject);
    }
}