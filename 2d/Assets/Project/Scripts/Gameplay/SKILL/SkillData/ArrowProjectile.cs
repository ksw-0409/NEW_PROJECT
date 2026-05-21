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

        enemy.TakeDamage(dmg);

        if (bowSkillRef != null && !isRicochet)
        {
            bowSkillRef.NotifyArrowHit(isCrit);
        }

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
        // ⭐ 화염구의 폭발 이펙트와 동일한 방식으로 크기를 폭발 반경에 비례하게 스폰
        if (fireFieldPrefab != null)
        {
            GameObject fx = Instantiate(fireFieldPrefab, pos, Quaternion.identity);
            const float spriteNative = 0.48f;
            const float activeRatio  = 0.9f;
            float fxSize = (explosionRadius * 2f) / (spriteNative * activeRatio);
            fx.transform.localScale = new Vector3(fxSize, fxSize, 1f);
            Destroy(fx, 0.55f);
        }

        // 카메라 흔들림 (있다면)
        if (CameraShake.Instance != null)
            CameraShake.ShakePreset(CameraShake.Preset.Light);

        // 폭발 범위 시각화 (있다면)
        SkillRangeIndicator.Spawn(
            pos,
            explosionRadius,
            new Color(1f, 0.55f, 0.1f, 0.95f),
            0.5f,
            SkillRangeIndicator.Shape.Circle
        );

        // 폭발 데미지
        foreach (var col in Physics2D.OverlapCircleAll(pos, explosionRadius))
        {
            if (col.CompareTag("Enemy"))
                col.GetComponent<EnemyHealth>()?.TakeDamage(extraDamage);
        }
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
        // if (mv != null) mv.ApplySlow(iceSlowAmount, iceDuration);
        // (얼음 둔화 적용 부분이 주석 처리되어 있어 실제 효과 없음 — EnemyAI에 ApplySlow가 있으면 활성화 필요)
    }
}
