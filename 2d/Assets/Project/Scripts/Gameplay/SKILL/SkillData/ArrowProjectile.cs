using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArrowProjectile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject fireFieldPrefab;

    [Header("기본 화살 스프라이트 (패시브 미발동 시 이 이미지 사용)")]
    public Sprite defaultArrowSprite;
    public Color  defaultArrowColor = Color.white;

    [Header("Life Settings")]
    public float lifeTime = 3f;

    // ─── 내부 상태 ───────────────────────────
    private float baseDamage;
    private float extraDamage   = 0;
    private int   pierceCount   = 0;
    private float damageDecay   = 1.0f;

    private List<string> appliedPassives = new List<string>();

    private float explosionRadius    = 0;
    private float poisonDmgPerTick   = 0;
    private float poisonDuration     = 0;
    private float iceSlowAmount      = 0;
    private float iceDuration        = 0;

    private bool   useAllowedArea;
    private Vector2 allowedAreaCenter;
    private float  allowedAreaRadius;

    // ✨ 활쏘기 특수효과 필드
    [HideInInspector] public float weakpointCritBoost = 0f;     // 다음 화살 치명타 데미지 +x
    [HideInInspector] public bool  ricochetEnabled = false;     // 치명타 시 주변 적으로 도탄
    [HideInInspector] public BowSkill bowSkillRef;              // 치명타 적중 알림 용
    [HideInInspector] public bool  isRicochet = false;          // 이 화살이 도탄된 화살인지

    private HashSet<EnemyHealth> alreadyHit = new HashSet<EnemyHealth>();

    // ─── 초기화 ──────────────────────────────
    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (defaultArrowSprite == null && spriteRenderer != null)
            defaultArrowSprite = spriteRenderer.sprite;
        if (defaultArrowColor == Color.white && spriteRenderer != null)
            defaultArrowColor = spriteRenderer.color;
    }

    void Start() => Destroy(gameObject, lifeTime);

    // ─── 세팅 ────────────────────────────────
    public void Setup(float baseDmg, float multiplier, float speed, Vector2 dir)
    {
        baseDamage = baseDmg * multiplier;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * speed;

        ResetVisual();
    }

    public void SetPierce(SkillLevelData ld)
    {
        pierceCount = Mathf.RoundToInt(ld.count);
        damageDecay = ld.multiplier;
    }

    public void SetAllowedArea(Vector2 center, float radius)
    {
        useAllowedArea    = true;
        allowedAreaCenter = center;
        allowedAreaRadius = Mathf.Max(0.1f, radius);
    }

    // ─── 패시브 추가 ─────────────────────────
    public void AddPassive(SkillLevelData ld, string type, Color color, Sprite newSprite = null)
    {
        if (appliedPassives.Contains(type)) return;
        appliedPassives.Add(type);

        bool isFirst = appliedPassives.Count == 1;
        if (isFirst && spriteRenderer != null)
        {
            if (newSprite != null) spriteRenderer.sprite = newSprite;
            spriteRenderer.color = color;
        }

        switch (type)
        {
            case "Explosion":
                explosionRadius = ld.explosionRadius;
                extraDamage    += ld.damage * ld.multiplier;
                break;
            case "Poison":
                float totalStat  = PlayerStats.Instance.data.physicalDamage
                                 + PlayerStats.Instance.data.magicDamage;
                poisonDmgPerTick = ld.damage + totalStat * 0.1f;
                poisonDuration   = ld.duration;
                break;
            case "Ice":
                iceSlowAmount = ld.multiplier;
                iceDuration   = ld.duration;
                break;
        }
    }

    public void ResetVisual()
    {
        if (spriteRenderer == null) return;
        if (defaultArrowSprite != null) spriteRenderer.sprite = defaultArrowSprite;
        spriteRenderer.color = defaultArrowColor;
    }

    // ─── 충돌 ────────────────────────────────
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;

        if (useAllowedArea)
        {
            float dist = Vector2.Distance(col.transform.position, allowedAreaCenter);
            if (dist > allowedAreaRadius) return;
        }

        var enemy = col.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        // 같은 적 중복 타격 방지
        if (alreadyHit.Contains(enemy)) return;
        alreadyHit.Add(enemy);

        // ✨ 치명타 판정
        bool isCrit = false;
        if (PlayerStats.Instance != null)
        {
            float critChance = PlayerStats.Instance.CriticalChance;
            if (Random.value < critChance) isCrit = true;
        }

        float dmg = baseDamage + extraDamage;

        if (isCrit)
        {
            float critMul = PlayerStats.Instance != null ? PlayerStats.Instance.CriticalDamage : 1.5f;
            if (critMul < 1.01f) critMul = 1.5f; // 기본 치명타 배율
            // ✨ 약점 사격 보너스 적용
            critMul += weakpointCritBoost;
            dmg *= critMul;
        }

        enemy.TakeDamage(dmg);

        // ✨ BowSkill에 치명타 알림 (약점 사격용)
        if (bowSkillRef != null && !isRicochet)
        {
            bowSkillRef.NotifyArrowHit(isCrit);
        }

        // ✨ 화살 도탄: 치명타 시 주변 적으로 튕김 (도탄 화살은 다시 도탄 안 함)
        if (isCrit && ricochetEnabled && !isRicochet)
        {
            TriggerRicochet(enemy);
        }

        if (appliedPassives.Contains("Explosion")) DoExplosion(col.transform.position);
        if (appliedPassives.Contains("Poison") && enemy.gameObject.activeInHierarchy) ApplyPoison(enemy);
        if (appliedPassives.Contains("Ice")) ApplyIce(enemy);

        if (pierceCount > 0) { pierceCount--; baseDamage *= damageDecay; }
        else Destroy(gameObject);
    }

    // ✨ 도탄: 가장 가까운 다른 적에게 새 화살을 발사
    void TriggerRicochet(EnemyHealth currentTarget)
    {
        // 반경 4유닛 내 다른 적 찾기
        float searchRadius = 4f;
        var hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        EnemyHealth bestTarget = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            var e = h.GetComponent<EnemyHealth>();
            if (e == null || e == currentTarget) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < bestDist) { bestDist = d; bestTarget = e; }
        }
        if (bestTarget == null) return;

        // 새 도탄 화살 인스턴스 생성
        GameObject ricochetGo = Instantiate(gameObject, transform.position, Quaternion.identity);
        ricochetGo.transform.localScale = transform.localScale;

        var ricoArrow = ricochetGo.GetComponent<ArrowProjectile>();
        if (ricoArrow != null)
        {
            // 패시브 효과는 절반만 적용
            Vector2 toTarget = ((Vector2)bestTarget.transform.position - (Vector2)transform.position).normalized;
            ricoArrow.Setup(baseDamage * 0.5f, 1f, 12f, toTarget);
            ricoArrow.isRicochet = true;
            ricoArrow.ricochetEnabled = false; // 한 번만 튕김
            ricoArrow.weakpointCritBoost = weakpointCritBoost;
        }

        // 방향 회전
        float angle = Mathf.Atan2(
            (bestTarget.transform.position.y - transform.position.y),
            (bestTarget.transform.position.x - transform.position.x)) * Mathf.Rad2Deg;
        ricochetGo.transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log("[BowRicochet] 화살 도탄 → " + bestTarget.name);
    }

    void DoExplosion(Vector2 pos)
    {
        if (fireFieldPrefab != null)
            Destroy(Instantiate(fireFieldPrefab, pos, Quaternion.identity), 1f);

        foreach (var col in Physics2D.OverlapCircleAll(pos, explosionRadius))
            if (col.CompareTag("Enemy"))
                col.GetComponent<EnemyHealth>()?.TakeDamage(extraDamage);
    }

    void ApplyPoison(EnemyHealth enemy)
    {
        if (enemy.gameObject.activeInHierarchy)
            enemy.StartCoroutine(PoisonRoutine(enemy));
    }

    IEnumerator PoisonRoutine(EnemyHealth enemy)
    {
        float elapsed = 0f;
        while (elapsed < poisonDuration && enemy != null && enemy.gameObject.activeInHierarchy)
        {
            enemy.TakeDamage(poisonDmgPerTick);
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    void ApplyIce(EnemyHealth enemy)
    {
        var mv = enemy.GetComponent<EnemyAI>();
        //   if (mv != null) mv.ApplySlow(iceSlowAmount, iceDuration);
        Debug.Log($"{enemy.name} 빙결 둔화 {iceSlowAmount*100:F0}%");
    }
}
