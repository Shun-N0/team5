using UnityEngine;

public class StunBullet : MonoBehaviour
{
    public float speed = 5f;
    public float stunDuration = 2.0f; 

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        if (transform.position.y < -6f) Destroy(gameObject);
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