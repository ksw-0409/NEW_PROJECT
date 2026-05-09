using UnityEngine;

/// <summary>
/// 보스 산탄 발사체 — 직선 비행 + 플레이어 충돌 시 데미지
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class WoodNeedle : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 dir;
    private float lifeTime = 4f;
    private bool initialized = false;

    public void Setup(float damage, float speed, Vector2 dir)
    {
        this.damage = damage;
        this.speed = speed;
        this.dir = dir.normalized;
        initialized = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = this.dir * speed;
            rb.gravityScale = 0f;
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerController>();
            if (ph == null) ph = other.GetComponentInParent<PlayerController>();
            if (ph != null) ph.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
