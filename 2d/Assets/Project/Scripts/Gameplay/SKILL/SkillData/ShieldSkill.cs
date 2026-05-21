using UnityEngine;
using System.Collections;

/// <summary>
/// [방어 스킬] 스킬트리 specialty 전부 지원
///   DEF_overload   : 방패 활성 중 마법 데미지 +30%
///   DEF_phase      : 방패 활성 중 이동속도 +20%, 적 통과
///   DEF_reflect    : 피격 시 받은 피해 300%를 주변 적에게 반사
///   DEF_double     : 방어막 최대 2개
///   DEF_sanctuary  : 방패 활성 중 초당 최대체력 3% 회복
/// </summary>
public class ShieldSkill : SkillBase
{
    // ── 데이터 ──────────────────────────────
    private ShieldData shieldData;

    // ── 상태 ───────────────────────────────
    private int   shieldCount    = 0;
    private int   maxShieldCount = 1;
    private bool  IsActive => shieldCount > 0;

    // ── 이펙트 ─────────────────────────────
    private GameObject shieldEffect;

    // ── 코루틴 ────────────────────────────
    private Coroutine sanctuaryRoutine;

    // ── 이동속도 캐시 ─────────────────────
    private bool  phaseApplied = false;
    private const float PHASE_SPEED_BONUS = 0.2f;   // +20%

    // ═══════════════════════════════════════
    // Init
    // ═══════════════════════════════════════
    public void Init(ShieldData data, SkillInstance inst)
    {
        base.Init(inst);
        shieldData = data;
    }

    // SkillBase.Init 오버로드 (PlayerSkillController 기존 코드 호환)
    public new void Init(SkillInstance inst)
    {
        base.Init(inst);
        // ShieldData는 나중에 SetData로 주입하거나 인스펙터 연결
    }

    // ═══════════════════════════════════════
    // AutoCast (부모 코루틴 사용)
    // ═══════════════════════════════════════
    protected override void Execute(Transform player)
    {
        // 이중 방어 specialty 체크 (런타임 해금 대응)
        maxShieldCount = Stats.HasSpecialty("DEF_double") ? 2 : 1;

        if (shieldCount >= maxShieldCount) return;

        shieldCount++;
        Debug.Log($"[Shield] 방어막 생성 ({shieldCount}/{maxShieldCount})");

        SpawnEffect(player);
        ApplyPassiveEffects();
    }

    // ═══════════════════════════════════════
    // 피격 시 호출 (PlayerController → OnHit)
    // ═══════════════════════════════════════
    /// <returns>방어막이 피해를 막았으면 true</returns>
    public bool OnHit(float incomingDamage)
    {
        if (!IsActive) return false;

        // [공격] 마나 반사
        if (Stats.HasSpecialty("DEF_reflect"))
            ReflectDamage(incomingDamage);

        shieldCount--;
        Debug.Log($"[Shield] 피격 흡수 → 남은 방어막 {shieldCount}");

        if (shieldCount <= 0)
            RemoveShield();

        return true;
    }

    // ─── 하위 함수 ─────────────────────────
    private void ApplyPassiveEffects()
    {
        // [공격] 마력 과부하: 방패 활성 중 마법 데미지 +30%
        // → PlayerStats에 임시 보너스를 넣는 방식 대신
        //    HasSpecialty 체크로 스킬 Execute 내에서 곱함 (FireballSkill 등이 직접 참조)
        //    별도 처리 없이 specialty 태그만 활성화된 상태면 됨

        // [유틸] 위상 변화: 이동속도 +20% & 적 통과
        if (Stats.HasSpecialty("DEF_phase") && !phaseApplied)
        {
          //  Stats.AddTemporarySpeedBonus(PHASE_SPEED_BONUS);
            phaseApplied = true;
            var rb = GetComponentInParent<Rigidbody2D>();
            if (rb != null) rb.excludeLayers = LayerMask.GetMask("Enemy");
            Debug.Log("[Shield] 위상 변화: 이동속도 +20%, 적 통과 ON");
        }

        // [변칙] 성역: 초당 최대체력 3% 회복
        if (Stats.HasSpecialty("DEF_sanctuary"))
        {
            if (sanctuaryRoutine != null) StopCoroutine(sanctuaryRoutine);
            sanctuaryRoutine = StartCoroutine(SanctuaryRegen());
        }
    }

    private void RemoveShield()
    {
        if (shieldEffect != null) { Destroy(shieldEffect); shieldEffect = null; }

        // 위상 변화 해제
        if (phaseApplied)
        {
          //  Stats.AddTemporarySpeedBonus(-PHASE_SPEED_BONUS);
            phaseApplied = false;
            var rb = GetComponentInParent<Rigidbody2D>();
            if (rb != null) rb.excludeLayers = 0;
            Debug.Log("[Shield] 위상 변화 해제");
        }

        // 성역 해제
        if (sanctuaryRoutine != null) { StopCoroutine(sanctuaryRoutine); sanctuaryRoutine = null; }

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

    private void SpawnEffect(Transform player)
    {
        // ✨ 옛날 쉴드 비주얼 비활성화 — PlayerShieldVisual의 새 마법진이 통일된 비주얼 제공
        // (shieldData.effectPrefab은 이제 안 씀)
    }

    private IEnumerator SanctuaryRegen()
    {
        while (IsActive)
        {
            yield return new WaitForSeconds(1f);
            float heal = Stats.MaxHealth * 0.03f;
            Stats.currentHealth = Mathf.Min(Stats.currentHealth + heal, Stats.MaxHealth);
            Debug.Log($"[Shield] 성역 회복: +{heal:F1}");
        }
    }

    // ═══════════════════════════════════════
    // 외부 참조용 프로퍼티
    // ═══════════════════════════════════════
    public bool  HasShield   => IsActive;
    public int   ShieldCount => shieldCount;

    // [공격] 마력 과부하 배율 (FireballSkill 등에서 bonus.dmg에 곱함)
    public float MagicDamageBonus => (IsActive && Stats.HasSpecialty("DEF_overload")) ? 1.3f : 1f;

    // ── 편의 프로퍼티 ──────────────────────
    private PlayerStats Stats => PlayerStats.Instance;
}
