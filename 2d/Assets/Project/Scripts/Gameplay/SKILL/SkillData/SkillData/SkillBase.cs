using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    protected SkillData data;
    protected SkillInstance instance;

    public void Init(SkillInstance instance)
    {
        this.instance = instance;
    }

    protected virtual void Start()
    {
        StartCoroutine(AutoCast());
    }

    protected abstract void Execute(Transform player);

    protected IEnumerator AutoCast()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetCooldown());
            Execute(this.transform);

            // ⭐ 전설 어빌리티 2 (카두케우스): 마법 스킬 시 10% 확률 즉시 재시전
            if (IsMagicSkill() && PlayerStats.Instance != null && PlayerStats.Instance.HasAbility(2))
            {
                if (UnityEngine.Random.value < 0.1f)
                {
                    Debug.Log("<color=magenta>[카두케우스]</color> 마법 즉시 재시전!");
                    Execute(this.transform);
                }
            }
        }
    }

    /// <summary>스킬 트리 노드 보너스를 가져옴 (data null이면 기본값 1/1/1/0/1/1)</summary>
    protected (float dmg, float rng, float cool, int cnt, float slowMul, float durMul) GetBonus()
    {
        // ⭐ data 필드가 NULL일 수 있음 — instance.data를 폴백으로 사용
        SkillData sd = data;
        if (sd == null && instance != null) sd = instance.data;
        // PlayerStats.Instance가 NULL이어도 FindFirst로 폴백 (씬 전환 직후 일시적 NULL 방지)
        var ps = PlayerStats.Instance;
        if (ps == null) ps = UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
        if (ps != null && sd != null)
            return ps.GetSkillBonus(sd);
        return (1f, 1f, 1f, 0, 1f, 1f);
    }

    /// <summary>최종 데미지 = (base × multiplier) × 보너스</summary>
    protected float GetDamage()
    {
        var ld = instance.GetCurrentLevelData();
        var b = GetBonus();
        return ld.damage * ld.multiplier * b.dmg;
    }

    /// <summary>최종 카운트 = base + 보너스</summary>
    protected int GetCount()
    {
        var ld = instance.GetCurrentLevelData();
        var b = GetBonus();
        return Mathf.Max(1, ld.count + b.cnt);
    }

    /// <summary>최종 쿨다운 = base × cooldownMul (작을수록 좋음, 최소 0.1초)</summary>
    protected float GetCooldown()
    {
        var ld = instance.GetCurrentLevelData();
        var b = GetBonus();
        return Mathf.Max(0.1f, ld.cooldown * b.cool);
    }

    /// <summary>⭐ NEW — 최종 범위 = base × rangeMul</summary>
    protected float GetRange()
    {
        var ld = instance.GetCurrentLevelData();
        var b = GetBonus();
        return ld.range * b.rng;
    }

    /// <summary>⭐ NEW — 최종 둔화율 (0~1, multiplier 곱)</summary>
    protected float GetSlowPercent()
    {
        var ld = instance.GetCurrentLevelData();
        var b = GetBonus();
        return Mathf.Clamp01(ld.slowPercent * b.slowMul);
    }

    /// <summary>⭐ NEW — 최종 지속시간 = base × durMul</summary>
    protected float GetDuration()
    {
        var ld = instance.GetCurrentLevelData();
        var b = GetBonus();
        return Mathf.Max(0.05f, ld.duration * b.durMul);
    }

    public int GetLevel() { return instance.level; }
    public bool IsMaxLevel() { return instance.IsMaxLevel(); }
    public void LevelUp() { instance.LevelUp(); }

    /// <summary>현재 스킬이 마법 계열인지 (카두케우스 재시전 트리거용)</summary>
    protected bool IsMagicSkill()
    {
        if (data == null) return false;
        string tn = data.GetType().Name;
        return tn == "FireballData" || tn == "ChainLightningData" || tn == "MeteorData" || tn == "IceRainData";
    }
}
