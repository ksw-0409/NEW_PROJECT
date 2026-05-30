using UnityEngine;
using System.Collections;

/// <summary>
/// [방어 스킬]
///   DEF_overload   : 방패 활성 중 마법 데미지 +30%
///   DEF_phase      : 방패 활성 중 이동속도 +20%, 적 통과
///   DEF_reflect    : 피격 시 받은 피해 300%를 주변 적에게 반사
///   DEF_double     : 방어막 최대 2개
///   (신성한 영역 회복은 PlayerShieldRegen이 담당 — currentShield > 0이면 무조건 회복)
/// </summary>
public class ShieldSkill : SkillBase
{

    // ⭐ 방어 스킬은 적이 없어도 발동돼야 함 (쉴드/신성한 영역 등은 패시브 성격)
    protected override bool RequiresEnemiesToCast() => false;
    private ShieldData shieldData;

    private int shieldCount    = 0;
    private int maxShieldCount = 1;
    private bool IsActive => shieldCount > 0;

    private GameObject shieldEffect;
    private bool phaseApplied = false;

    public void Init(ShieldData data, SkillInstance inst)
    {
        base.Init(inst);
        shieldData = data;
    }

    public new void Init(SkillInstance inst)
    {
        base.Init(inst);
    }

    protected override void Execute(Transform player)
    {
        maxShieldCount = (Stats != null && Stats.HasSpecialty("DEF_double")) ? 2 : 1;

        if (shieldCount >= maxShieldCount) return;

        shieldCount++;
        Debug.Log($"[Shield] 방어막 생성 ({shieldCount}/{maxShieldCount})");

        ApplyPassiveEffects();
    }

    /// <returns>방어막이 피해를 막았으면 true</returns>
    public bool OnHit(float incomingDamage)
    {
        if (!IsActive) return false;

        if (Stats != null && Stats.HasSpecialty("DEF_reflect"))
            ReflectDamage(incomingDamage);

        shieldCount--;
        Debug.Log($"[Shield] 피격 흡수 → 남은 방어막 {shieldCount}");

        if (shieldCount <= 0)
            RemoveShield();

        return true;
    }

    private void ApplyPassiveEffects()
    {
        // [유틸] 위상 변화: 이동속도 +20% & 적 통과
        if (Stats != null && Stats.HasSpecialty("DEF_phase") && !phaseApplied)
        {
            phaseApplied = true;
            var rb = GetComponentInParent<Rigidbody2D>();
            if (rb != null) rb.excludeLayers = LayerMask.GetMask("Enemy");
            Debug.Log("[Shield] 위상 변화: 이동속도 +20%, 적 통과 ON");
        }
        // 신성한 영역(HP 회복)은 PlayerShieldRegen이 currentShield > 0 조건으로 자동 처리
    }

    private void RemoveShield()
    {
        if (shieldEffect != null) { Destroy(shieldEffect); shieldEffect = null; }

        if (phaseApplied)
        {
            phaseApplied = false;
            var rb = GetComponentInParent<Rigidbody2D>();
            if (rb != null) rb.excludeLayers = 0;
            Debug.Log("[Shield] 위상 변화 해제");
        }

        Debug.Log("[Shield] 방어막 소멸");
    }

    private void ReflectDamage(float dmg)
    {
        float reflect = dmg * 3f;
        var hits = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (var h in hits)
            if (h.CompareTag("Enemy"))
                h.GetComponent<EnemyHealth>()?.TakeDamage(reflect);
        Debug.Log($"[Shield] 마나 반사: {reflect:F1} 데미지");
    }

    public bool HasShield => IsActive;
    public int ShieldCount => shieldCount;
    public float MagicDamageBonus => (IsActive && Stats != null && Stats.HasSpecialty("DEF_overload")) ? 1.3f : 1f;

    private PlayerStats Stats => PlayerStats.Instance;
}