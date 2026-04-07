using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private float damage;
    private float explosionRadius;
    private GameObject effectPrefab;

    private Vector3 startPos;
    private float maxDistance = 6f;

    private bool exploded = false; // ⭐ 중복 방지

    public void Init(float dmg, float radius, GameObject effect)
    {
        damage = dmg;
        explosionRadius = radius;
        effectPrefab = effect;

        startPos = transform.position;
    }

    void Update()
    {
        float dist = Vector3.Distance(startPos, transform.position);

        if (dist >= maxDistance)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded) return;

        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        // ⭐ 범위 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        // ⭐ 폭발 이펙트
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.5f);
        }

        Destroy(gameObject); // ⭐ 여기서 제거
    }
}