using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private float damage;
    private float explosionRadius;
    private GameObject effectPrefab;
    private float effectScale;

    private Vector3 startPos;
    private float maxDistance = 20f;
    private bool exploded = false;
    private bool isSplitChild = false;

    private void Awake() => startPos = transform.position;

    public void Init(float dmg, float radius, GameObject fx, float scale, bool isChild = false)
    {
        damage = dmg;
        explosionRadius = radius; // FireballSkill에서 받은 레벨 데이터 값이 들어옴
        effectPrefab = fx;
        effectScale = scale;
        this.isSplitChild = isChild;
    }

    void Update()
    {
        if (!exploded && Vector3.Distance(startPos, transform.position) > maxDistance) Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!exploded && other.CompareTag("Enemy")) Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        bool isBigExplosion = PlayerStats.Instance.HasSpecialty("Fireball_2_2");

        // 1. 판정 범위 (흰색 원 크기만큼 적 탐색)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float finalDmg = damage;
                if (isBigExplosion)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < explosionRadius * 0.4f) finalDmg *= 2f;
                }
                hit.GetComponent<EnemyHealth>()?.TakeDamage(finalDmg);
            }
        }

        // 2. 분열 (1회 제한)
        if (!isSplitChild && PlayerStats.Instance.HasSpecialty("Fireball_3_2")) SpawnSplitProjectiles();

        // 3. 이펙트 생성 및 크기 보정
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            // 프리팹 크기에 맞춰 보정 (9배가 적당하다고 하셨으므로 9f 적용)
            float fxSize = explosionRadius * 9f;
            fx.transform.localScale = new Vector3(fxSize, fxSize, 1f);
            Destroy(fx, 0.7f);
        }

        Destroy(gameObject);
    }

    void SpawnSplitProjectiles()
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            Vector3 splitDir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject tiny = Instantiate(gameObject, transform.position, Quaternion.identity);
            float tinyScale = transform.localScale.x * 0.6f;
            tiny.transform.localScale = Vector3.one * tinyScale;

            FireballProjectile tinyProj = tiny.GetComponent<FireballProjectile>();
            if (tinyProj != null) tinyProj.Init(damage * 0.5f, explosionRadius * 0.5f, effectPrefab, tinyScale, true);

            Rigidbody2D rb = tiny.GetComponent<Rigidbody2D>();
            if (rb) rb.linearVelocity = splitDir * 7f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}