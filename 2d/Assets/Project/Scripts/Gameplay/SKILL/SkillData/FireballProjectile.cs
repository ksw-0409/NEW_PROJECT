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

        // 1. 판정 범위 (OverlapCircle explosionRadius)
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

        // 3. 폭발 이펙트 — ⭐ explosion-a sprite native(0.48), 활성 픽셀 90% → 월드 지름이 explosionRadius*2가 되도록
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            // sprite native 0.48, 활성 90% → 실제 보이는 영역 = scale * 0.48 * 0.9
            // 월드 지름 = explosionRadius*2 → scale = explosionRadius*2 / (0.48*0.9)
            const float spriteNative = 0.48f;
            const float activeRatio = 0.9f;
            float fxSize = (explosionRadius * 2f) / (spriteNative * activeRatio);
            fx.transform.localScale = new Vector3(fxSize, fxSize, 1f);
            Destroy(fx, 0.55f); // 폭발 애니 0.5초 + 약간 여유
        }

        // 4. ⭐ 피격범위 가시화 — 외곽선 + 반투명 채움으로 정확한 데미지 영역을 보여줌
        SkillRangeIndicator.Spawn(
            transform.position,
            explosionRadius,
            new Color(1f, 0.55f, 0.1f, 0.95f),
            0.55f,
            SkillRangeIndicator.Shape.Circle
        );

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
        // 실제 시스태파이 판정(OverlapCircle explosionRadius)과 일치
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}