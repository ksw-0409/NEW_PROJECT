using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private float damage;

    public void Init(Vector2 dir, float speed, float damage)
    {
        this.dir = dir;
        this.speed = speed;
        this.damage = damage;

        Destroy(gameObject, 3f);
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        other.GetComponent<EnemyHealth>()?.TakeDamage(damage);

        Destroy(gameObject);
    }
}