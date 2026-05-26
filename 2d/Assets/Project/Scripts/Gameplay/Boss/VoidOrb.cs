using UnityEngine;

/// <summary>
/// 10층 보스(공허의 대사제) — 보라 구체 투사체.
/// 직선 비행 + 플레이어 충돌 시 데미지. (WoodNeedle 패턴 기반)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class VoidOrb : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 dir;
    private float lifeTime = 6f;
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

        // 진행 방향으로 회전
        float ang = Mathf.Atan2(this.dir.y, this.dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, ang);

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
