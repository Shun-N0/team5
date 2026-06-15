using UnityEngine;

public class WarshipUnit : MonoBehaviour
{
    private WarshipFormation formation;
    private int currentHealth;
    private bool destroyed;

    public void Initialize(WarshipFormation owner, int health)
    {
        formation = owner;
        currentHealth = health;
    }

    public void Fire(GameObject bulletPrefab, float bulletSpeed, float bulletScale)
    {
        if (bulletPrefab == null || destroyed) return;

        Vector3 spawnPosition = transform.position + Vector3.down * 0.35f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bullet.transform.localScale = Vector3.one * bulletScale;
        bullet.SendMessage("SetSpeed", bulletSpeed, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (destroyed || !collision.CompareTag("Bullet")) return;

        Bullet bullet = collision.GetComponent<Bullet>();
        int damage = bullet != null ? bullet.attackPower : 1;
        Destroy(collision.gameObject);

        currentHealth -= damage;
        if (currentHealth > 0) return;

        destroyed = true;
        formation.OnUnitDestroyed(this);
        Destroy(gameObject);
    }
}
