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
    [HideInInspector] public float weakpointCritBoost = 0f;
    [HideInInspector] public bool  ricochetEnabled = false;
    [HideInInspector] public BowSkill bowSkillRef;
    [HideInInspector] public bool  isRicochet = false;

    private HashSet<EnemyHealth> alreadyHit = new HashSet<EnemyHealth>();

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

        // ⭐ [공격] Arrow_Pierce_penetrate: 특수 관통 — 관통 +1, 데미지 감쇄 없음
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Pierce_penetrate"))
        {
            pierceCount += 1;
            damageDecay = 1.0f; // 감쇄 없음
        }
        // ⭐ [유틸] Arrow_Pierce_soul: 영혼 관통 — 관통 +3
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Pierce_soul"))
        {
            pierceCount += 3;
        }
    }

    public void SetAllowedArea(Vector2 center, float radius)
    {
        useAllowedArea    = true;
        allowedAreaCenter = center;
        allowedAreaRadius = Mathf.Max(0.1f, radius);
    }

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
                float totalStat  = PlayerStats.Instance != null && PlayerStats.Instance.data != null
                                 ? PlayerStats.Instance.data.physicalDamage + PlayerStats.Instance.data.magicDamage
                                 : 0f;
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
            if (critMul < 1.01f) critMul = 1.5f;
            critMul += weakpointCritBoost;
            dmg *= critMul;
        }

        enemy.TakeDamage(dmg, isCrit); // ⭐ ArrowProjectile은 이미 crit 계산 — 이중 방지

        if (bowSkillRef != null && !isRicochet)
        {
            bowSkillRef.NotifyArrowHit(isCrit);
        }

        if (ricochetEnabled && !isRicochet) // ⭐ 치명타 조건 제거 — 모든 적중에 1회 튕김
        {
            TriggerRicochet(enemy);
        }

        if (appliedPassives.Contains("Explosion")) DoExplosion(col.transform.position);
        if (appliedPassives.Contains("Poison") && enemy.gameObject.activeInHierarchy) ApplyPoison(enemy);
        if (appliedPassives.Contains("Ice")) ApplyIce(enemy);

        if (pierceCount > 0) { pierceCount--; baseDamage *= damageDecay; }
        else Destroy(gameObject);
    }

    void TriggerRicochet(EnemyHealth currentTarget)
    {
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

        GameObject ricochetGo = Instantiate(gameObject, transform.position, Quaternion.identity);
        ricochetGo.transform.localScale = transform.localScale;

        var ricoArrow = ricochetGo.GetComponent<ArrowProjectile>();
        if (ricoArrow != null)
        {
            Vector2 toTarget = ((Vector2)bestTarget.transform.position - (Vector2)transform.position).normalized;
            ricoArrow.Setup(baseDamage * 0.5f, 1f, 12f, toTarget);
            ricoArrow.isRicochet = true;
            ricoArrow.ricochetEnabled = false;
            ricoArrow.weakpointCritBoost = weakpointCritBoost;
        }

        float angle = Mathf.Atan2(
            (bestTarget.transform.position.y - transform.position.y),
            (bestTarget.transform.position.x - transform.position.x)) * Mathf.Rad2Deg;
        ricochetGo.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 폭발화살 발동 시 화염구와 동일한 시각/판정 효과
    /// </summary>
    void DoExplosion(Vector2 pos)
    {
        // ⭐ [변칙] Arrow_Explosion_carpet: 융단 폭격 — 폭발 반경 +50%
        float effectiveRadius = explosionRadius;
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Explosion_carpet"))
            effectiveRadius *= 1.5f;

        // 화염 이펙트
        if (fireFieldPrefab != null)
        {
            GameObject fx = Instantiate(fireFieldPrefab, pos, Quaternion.identity);
            const float spriteNative = 0.48f;
            const float activeRatio  = 0.9f;
            float fxSize = (effectiveRadius * 2f) / (spriteNative * activeRatio);
            fx.transform.localScale = new Vector3(fxSize, fxSize, 1f);
            Destroy(fx, 0.55f);
        }
        if (CameraShake.Instance != null) CameraShake.ShakePreset(CameraShake.Preset.Light);
        SkillRangeIndicator.Spawn(pos, effectiveRadius, new Color(1f, 0.55f, 0.1f, 0.95f), 0.5f, SkillRangeIndicator.Shape.Circle);

        // 폭발 데미지
        var hits = Physics2D.OverlapCircleAll(pos, effectiveRadius);
        var hitEnemies = new System.Collections.Generic.List<EnemyHealth>();
        foreach (var col in hits)
        {
            if (col != null && col.CompareTag("Enemy"))
            {
                var e = col.GetComponent<EnemyHealth>();
                if (e != null)
                {
                    e.TakeDamage(extraDamage);
                    hitEnemies.Add(e);
                }
            }
        }

        // ⭐ [공격] Arrow_Explosion_chain: 연쇄 폭발 — 폭발 적 1명에서 2차 폭발 (반경 50%, 데미지 50%)
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Explosion_chain") && hitEnemies.Count > 0)
        {
            // 최초 적의 위치에서 2차 폭발
            var firstEnemy = hitEnemies[0];
            if (firstEnemy != null)
            {
                Vector2 chainPos = firstEnemy.transform.position;
                float chainRadius = effectiveRadius * 0.6f;
                float chainDmg = extraDamage * 0.5f;
                StartCoroutine(DelayedChainExplosion(chainPos, chainRadius, chainDmg, firstEnemy));
            }
        }
    }

    /// <summary>연쇄 폭발: 0.2초 후 2차 폭발 (시각적으로 분리)</summary>
    IEnumerator DelayedChainExplosion(Vector2 pos, float radius, float dmg, EnemyHealth skipTarget)
    {
        yield return new WaitForSeconds(0.2f);
        SkillRangeIndicator.Spawn(pos, radius, new Color(1f, 0.7f, 0.2f, 0.95f), 0.4f, SkillRangeIndicator.Shape.Circle);
        foreach (var col in Physics2D.OverlapCircleAll(pos, radius))
        {
            if (col == null || !col.CompareTag("Enemy")) continue;
            var e = col.GetComponent<EnemyHealth>();
            if (e != null && e != skipTarget) e.TakeDamage(dmg);
        }
    }

    void ApplyPoison(EnemyHealth enemy)
    {
        if (enemy.gameObject.activeInHierarchy)
            enemy.StartCoroutine(PoisonRoutine(enemy));

        // ⭐ [공격] Arrow_Poison_nerve: 신경 독소 — 독 적용 시 1.5초 30% 둔화
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Poison_nerve"))
        {
            var mv = enemy.GetComponent<EnemyAI>();
            if (mv != null) mv.ApplySlow(0.7f, 1.5f);
        }
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

        // ⭐ [변칙] Arrow_Poison_plague: 역병 — 독으로 적이 죽을 때 주변 적에게 독 전염
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
        {
            if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Poison_plague") && enemy != null)
            {
                Vector2 deathPos = enemy.transform.position;
                foreach (var col in Physics2D.OverlapCircleAll(deathPos, 2.5f))
                {
                    if (col == null || !col.CompareTag("Enemy")) continue;
                    var newTarget = col.GetComponent<EnemyHealth>();
                    if (newTarget != null && newTarget != enemy && newTarget.gameObject.activeInHierarchy)
                    {
                        newTarget.StartCoroutine(PoisonRoutine(newTarget));
                    }
                }
                SkillRangeIndicator.Spawn(deathPos, 2.5f, new Color(0.4f, 0.9f, 0.2f, 0.85f), 0.4f, SkillRangeIndicator.Shape.Circle);
            }
        }
    }

    void ApplyIce(EnemyHealth enemy)
    {
        // ⭐ 슬로우 활성화 (이전에 주석 처리되어 있었음)
        var mv = enemy.GetComponent<EnemyAI>();
        if (mv != null)
        {
            float slowAmt = iceSlowAmount;
            float slowDur = iceDuration;
            // [유틸] Arrow_Ice_absolute: 절대 영도 — 슬로우 강도/지속 2배
            if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Ice_absolute"))
            {
                slowAmt *= 2f;
                slowDur *= 2f;
            }
            // ApplySlow 시그니처: (multiplier 0~1, duration). slowAmt가 0.3이면 70% 속도로 둔화
            mv.ApplySlow(Mathf.Clamp(1f - slowAmt, 0.1f, 1f), slowDur);
        }

        // ⭐ [공격] Arrow_Ice_shatter: 쇄빙 — 빙결된 적 사망 시 주변에 얼음 폭발
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Arrow_Ice_shatter"))
        {
            enemy.StartCoroutine(IceShatterWatchdog(enemy));
        }
    }

    /// <summary>쇄빙: 얼음 적용 적이 죽을 때 주변에 작은 폭발</summary>
    IEnumerator IceShatterWatchdog(EnemyHealth enemy)
    {
        while (enemy != null && enemy.gameObject.activeInHierarchy && enemy.currentHp > 0f)
            yield return new WaitForSeconds(0.1f);

        if (enemy != null)
        {
            Vector2 pos = enemy.transform.position;
            float dmg = baseDamage * 0.6f;
            float radius = 1.5f;
            foreach (var col in Physics2D.OverlapCircleAll(pos, radius))
            {
                if (col != null && col.CompareTag("Enemy"))
                {
                    var e = col.GetComponent<EnemyHealth>();
                    if (e != null && e != enemy) e.TakeDamage(dmg);
                }
            }
            if (CameraShake.Instance != null) CameraShake.ShakePreset(CameraShake.Preset.Light);
            SkillRangeIndicator.Spawn(pos, radius, new Color(0.5f, 0.85f, 1f, 0.95f), 0.4f, SkillRangeIndicator.Shape.Circle);
        }
    }
}