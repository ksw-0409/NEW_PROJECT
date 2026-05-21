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

    private void Awake()
    {
        startPos = transform.position;
        // 5초 안전장치
        Invoke(nameof(ForceExpire), 5f);
    }

    void ForceExpire()
    {
        if (!exploded)
        {
            Explode();
        }
    }

    public void Init(float dmg, float radius, GameObject fx, float scale, bool isChild = false)
    {
        damage = dmg;
        explosionRadius = radius;
        effectPrefab = fx;
        effectScale = scale;
        this.isSplitChild = isChild;
    }

    void Update()
    {
        if (!exploded && Vector3.Distance(startPos, transform.position) > maxDistance)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!exploded && other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!exploded && collision.collider.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;
        CancelInvoke(nameof(ForceExpire));

        // 임팩트 흔들림
        CameraShake.ShakePreset(isSplitChild ? CameraShake.Preset.Light : CameraShake.Preset.Medium);

        bool isBigExplosion = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Fireball_2_2");

        // 1. 데미지 판정
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
        if (!isSplitChild && PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Fireball_3_2"))
            SpawnSplitProjectiles();

        // 3. 폭발 이펙트
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            const float spriteNative = 0.48f;
            const float activeRatio = 0.9f;
            float fxSize = (explosionRadius * 2f) / (spriteNative * activeRatio);
            fx.transform.localScale = new Vector3(fxSize, fxSize, 1f);
            Destroy(fx, 0.55f);
        }

        // 4. 피격범위 가시화
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
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
