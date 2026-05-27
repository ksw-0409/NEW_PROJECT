using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerStats : MonoBehaviour
{
    public PlayerData data;
    public static PlayerStats Instance;
    public event Action OnLevelUp;          // 레벨업 이벤트
    public static event Action OnPlayerDied; // 사망 이벤트

    public float currentHealth;
    public float currentLevel = 1;
    public float currentExp = 0;
    private bool isDead = false;

    // 장비 시스템
    private Dictionary<EquipmentSlot, EquipmentData> equippedItems = new Dictionary<EquipmentSlot, EquipmentData>();

    private Dictionary<SkillData, (float dmg, float rng, float cool, int cnt, float slowMul, float durMul)> skillBonuses = new();
    private HashSet<string> activeSpecialties = new HashSet<string>();

    // ============================================================
    // ✨ 쉴드 시스템 — 몬스터 접촉/투사체 피격 시 HP 대신 쉴드가 먼저 소비됨
    // ============================================================
    [Header("Shield")]
    [Tooltip("현재 쉴드 잔량 (0 이상이면 HP 대신 먼저 소비)")]
    public int currentShield = 0;
    [Tooltip("쉴드 최대 보유량 (DEF_double 해금 시 2)")]
    public int maxShield = 1;
    public event Action<int> OnShieldChanged;

    public bool ConsumeShield()
    {
        if (currentShield <= 0) return false;
        currentShield--;
        OnShieldChanged?.Invoke(currentShield);
        Debug.Log($"<color=#88ccff>[Shield]</color> 소비 → 잔량 {currentShield}/{maxShield}");

        // ✨ VFX + 카메라 흔들림
        VFXManager.SpawnShieldBreak(transform.position);
        CameraShake.ShakePreset(CameraShake.Preset.Medium);
        return true;
    }

    public void AddShield()
    {
        if (currentShield >= maxShield) return;
        currentShield++;
        OnShieldChanged?.Invoke(currentShield);
        Debug.Log($"<color=#88ccff>[Shield]</color> 추가 → 잔량 {currentShield}/{maxShield}");

        // ✨ 쉴드 재생 이펙트
        VFXManager.SpawnShieldRegen(transform.position);
    }

    public void SetMaxShield(int max)
    {
        maxShield = Mathf.Max(1, max);
        if (currentShield > maxShield) currentShield = maxShield;
        OnShieldChanged?.Invoke(currentShield);
    }

    // ============================================================
    // 헬퍼: 패시브 시스템 안전 접근
    // ============================================================
    private float PassiveMul(string id) =>
        PassiveSystem.Instance != null ? PassiveSystem.Instance.GetMultiplier(id) : 1f;
    private float PassiveBonus(string id) =>
        PassiveSystem.Instance != null ? PassiveSystem.Instance.GetBonus(id) : 0f;
    private float DebtDmgMul =>
        PassiveSystem.Instance != null ? PassiveSystem.Instance.DebtDamageMultiplier : 1f;
    private float DebtSpdMul =>
        PassiveSystem.Instance != null ? PassiveSystem.Instance.DebtSpeedMultiplier : 1f;

    #region Properties (장비 합산 + 패시브 보너스 + 디버프)
    // 최대 체력: data + 장비 합산, ×(1 + 패시브 HP 보너스)
    public float MaxHealth => (data.maxHealth + GetEquipSum(item => item.maxHealth)) * PassiveMul(PassiveSystem.ID_HP);

    // 이동 속도: data + 장비 합산, ×(1 + 패시브 이속 보너스), ×채무자 페널티(있으면 0.85)
    public float MoveSpeed => (data.moveSpeed + GetEquipSum(item => item.moveSpeed)) * PassiveMul(PassiveSystem.ID_SPEED) * DebtSpdMul;

    // 물리 데미지: data + 장비 합산, ×(1 + 패시브 물리 데미지 보너스), ×채무자 페널티(0.7)
    public float PhysicalDamage => (data.physicalDamage + GetEquipSum(item => item.physicalDamage)) * PassiveMul(PassiveSystem.ID_PHYS_DMG) * DebtDmgMul;

    // 마법 데미지
    public float MagicDamage => (data.magicDamage + GetEquipSum(item => item.magicDamage)) * PassiveMul(PassiveSystem.ID_MAG_DMG) * DebtDmgMul;

    // 방어 (data 기준 그대로 + 장비)
    public float PhysicalDefense => data.physicalDefense + GetEquipSum(item => item.physicalDefense);

    // 방어율: data + 장비 + 패시브 방어 보너스 (받는 피해 감소율)
    public float DefenseRate => data.defenseRate + GetEquipSum(item => item.defense) + PassiveBonus(PassiveSystem.ID_DEFENSE);

    // 치명타 확률: data + 장비 + 패시브 치명타 보너스
    public float CriticalChance => data.criticalChance + GetEquipSum(item => item.criticalChance) + PassiveBonus(PassiveSystem.ID_CRIT);

    public float CriticalDamage => data.criticalDamage + GetEquipSum(item => item.criticalDamage);
    public float AttackCooldown => data.attackcooldown + GetEquipSum(item => item.moveSpeed);
    #endregion

    void Awake()
    {
        if (Instance == null) Instance = this;
        currentHealth = data.maxHealth;

#if UNITY_EDITOR
        // 에디터 테스트: 쉴드 자동 부여 (방어 패시브 레벨 0이어도 쉴드 작동 확인 가능)
        // PassiveSystem이 아직 없을 수 있으므로 다음 프레임에 처리
        Invoke("GiveTestShield", 0.5f);
#endif
    }

    void GiveTestShield()
    {
        // 패시브 방어 레벨이 0이면 자동으로 1개 쉴드 부여 (테스트 편의)
        if (PassiveSystem.Instance != null && PassiveSystem.Instance.GetLevel(PassiveSystem.ID_SHIELD) == 0)
        {
            currentShield = 1;
            OnShieldChanged?.Invoke(currentShield);
            Debug.Log($"<color=#88ccff>[Shield Test]</color> 에디터 테스트용 쉴드 1개 자동 부여");
        }
    }

    void Start()
    {
        RestoreUnlockedEffectsFromSave();

        // PassiveSystem이 같은 GameObject에 없으면 자동 추가 (보존성 보장)
        if (GetComponent<PassiveSystem>() == null)
        {
            gameObject.AddComponent<PassiveSystem>();
        }
    }

    void RestoreUnlockedEffectsFromSave()
    {
        if (GameDataManager.Instance == null) return;
        var effects = GameDataManager.Instance.GetUnlockedEffects();
        if (effects == null || effects.Count == 0) return;

        var allSkillData = Resources.FindObjectsOfTypeAll<SkillData>();
        var skillByName = new Dictionary<string, SkillData>();
        foreach (var sd in allSkillData)
        {
            if (sd != null && !string.IsNullOrEmpty(sd.name) && !skillByName.ContainsKey(sd.name))
                skillByName[sd.name] = sd;
        }

        int restored = 0;
        foreach (var e in effects)
        {
            SkillData skill = null;
            if (!string.IsNullOrEmpty(e.targetSkillAssetName))
                skillByName.TryGetValue(e.targetSkillAssetName, out skill);

            if (e.nodeType == 1)
            {
                UnlockSpecialty(e.specialtyTag, skill,
                    e.damageMultiplier, e.rangeMultiplier, e.cooldownMultiplier,
                    e.countBonus, e.slowPercentMultiplier, e.durationMultiplier);
                restored++;
            }
            else if (skill != null)
            {
                UpdateSkillBonus(skill,
                    e.damageMultiplier, e.rangeMultiplier, e.cooldownMultiplier,
                    e.countBonus, e.slowPercentMultiplier, e.durationMultiplier);
                restored++;
            }
        }

        Debug.Log("<color=cyan>[PlayerStats]</color> saved " + effects.Count + " effects, restored " + restored);
    }

    public bool HasSpecialty(string specialtyTag) => activeSpecialties.Contains(specialtyTag);

    public bool HasAnySpecialty(params string[] specialtyTags)
    {
        if (specialtyTags == null) return false;
        for (int i = 0; i < specialtyTags.Length; i++)
            if (!string.IsNullOrEmpty(specialtyTags[i]) && activeSpecialties.Contains(specialtyTags[i]))
                return true;
        return false;
    }

    public bool HasAnySpecialtyContains(params string[] keywordTags)
    {
        if (keywordTags == null || keywordTags.Length == 0) return false;
        foreach (string active in activeSpecialties)
        {
            if (string.IsNullOrEmpty(active)) continue;
            for (int i = 0; i < keywordTags.Length; i++)
                if (!string.IsNullOrEmpty(keywordTags[i]) && active.Contains(keywordTags[i], StringComparison.OrdinalIgnoreCase))
                    return true;
        }
        return false;
    }

    public void UnlockSpecialty(string specialtyTag, SkillData skillData, float d, float r, float c, int cnt,
        float slowMul = 1f, float durMul = 1f)
    {
        if (!string.IsNullOrEmpty(specialtyTag))
        {
            activeSpecialties.Add(specialtyTag);
            Debug.Log($"<color=yellow>[특수 효과 해금]</color> 태그: {specialtyTag}");

            // ✨ DEF_double 효과: 쉴드 최대 보유 2개로
            if (specialtyTag == "DEF_double") SetMaxShield(2);
        }

        UpdateSkillBonus(skillData, d, r, c, cnt, slowMul, durMul);
    }

    public void UpdateSkillBonus(SkillData skillData, float d, float r, float c, int cnt,
        float slowMul = 1f, float durMul = 1f)
    {
        if (skillData == null) return;

        if (!skillBonuses.ContainsKey(skillData))
            skillBonuses[skillData] = (1f, 1f, 1f, 0, 1f, 1f);

        var current = skillBonuses[skillData];
        skillBonuses[skillData] = (
            current.dmg * d,
            current.rng * r,
            current.cool * c,
            current.cnt + cnt,
            Mathf.Max(0.01f, current.slowMul * slowMul),
            Mathf.Max(0.01f, current.durMul * durMul)
        );
    }

    public (float dmg, float rng, float cool, int cnt, float slowMul, float durMul) GetSkillBonus(SkillData skillData)
    {
        if (skillData != null && skillBonuses.ContainsKey(skillData))
            return skillBonuses[skillData];
        return (1f, 1f, 1f, 0, 1f, 1f);
    }

    private float GetEquipSum(System.Func<EquipmentData, float> statSelector)
    {
        float sum = 0.0f;
        foreach (var item in equippedItems.Values)
        {
            if (item != null) sum += statSelector(item);
        }
        return sum;
    }

    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;
        equippedItems[newItem.slot] = newItem;
        if (currentHealth > MaxHealth) currentHealth = MaxHealth;
        Debug.Log($"{newItem.itemName} 장착 완료. 현재 공격력: {PhysicalDamage}");
    }

    public void Unequip(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            equippedItems.Remove(slot);
            Debug.Log($"{slot} 슬롯 장비 해제");
        }
    }

    public void TakeExp(float exp)
    {
        // ✨ 경험치 패시브 적용
        float multiplier = PassiveMul(PassiveSystem.ID_EXP);
        currentExp += exp * multiplier;
        Debug.Log(currentExp);
        if (currentExp >= 2) LevelUp();
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;
        Debug.Log("Level++" + currentLevel);

        // ✨ 레벨업 이펙트
        VFXManager.SpawnLevelUp(transform.position);

        OnLevelUp?.Invoke();
    }

    // ============================================================
    // ✨ TakeDamage — 쉴드 먼저 소비 → 그 다음 HP
    // 몬스터 접촉, 적 투사체 모두 이 메서드를 거치도록 함
    // ============================================================
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (damage <= 0f) return;

        // ✨ 모든 데미지 시 카메라 흔들림 (쉴드든 HP든)
        CameraShake.ShakePreset(CameraShake.Preset.Light);

        // 쉴드가 있으면 모든 데미지를 흡수하고 쉴드 1개 소비
        if (currentShield > 0)
        {
            ConsumeShield();

            // [방어 변칙] 마나 반사: 받았어야 할 피해의 300%를 주변에 되돌림
            if (HasSpecialty("DEF_reflect"))
            {
                ReflectShieldDamage(damage * 3f);
            }
            return; // HP 안 깎임
        }

        // 패시브 방어 보너스만큼 데미지 감소
        float defenseBonus = PassiveBonus(PassiveSystem.ID_DEFENSE);
        damage *= Mathf.Max(0f, 1f - defenseBonus);

        currentHealth -= damage;

        // ✨ 피격 이펙트 (쉴드 없을 때, HP가 실제 깎인 경우) — Medium 흔들림 추가
        VFXManager.SpawnPlayerHurt(transform.position);
        CameraShake.ShakePreset(CameraShake.Preset.Medium);

        if (currentHealth < 0)
        {
            isDead = true;
            Die();
        }
    }

    // 쉴드 무시 데미지 (특수한 즉사 효과용 — 거의 안 씀)
    public void TakeFixedDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            isDead = true;
            Die();
        }
    }

    // [방어 변칙] 마나 반사 효과
    private void ReflectShieldDamage(float reflectDamage)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (var enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy")) continue;
            var eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(reflectDamage);
        }
        Debug.Log($"<color=#88ccff>[Shield 반사]</color> {reflectDamage} 데미지를 주변 적에게 반환");
    }

    private void Die()
    {
        Debug.Log("사망");
        OnPlayerDied?.Invoke();
        //  관성 제거 및 물리 연산 완전 차단
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 미끄러짐 방지 (즉시 정지)
            rb.simulated = false;             // 물리 엔진 개입 차단 (적에게 밀리지 않음)
        }

        GetComponent<PlayerAnimation>()?.PlayDie();
    }

    public void HealToFull()
    {
        currentHealth = MaxHealth;
        isDead = false;
    }
}
