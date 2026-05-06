using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;
public class PlayerStats : MonoBehaviour
{
    public PlayerData data;
    public static PlayerStats Instance;
    public event Action OnLevelUp; // 레벨업 이벤트 
    public static event Action OnPlayerDied; //사망 이벤트

    public float currentHealth;
    public float currentLevel = 1;
    public float currentExp = 0;
    private bool isDead = false;

    //data테이블에서 가져온 데이터들 

    // 장착한 장비 저장
    private Dictionary<EquipmentSlot, EquipmentData> equippedItems = new Dictionary<EquipmentSlot, EquipmentData>();

    private Dictionary<SkillData, (float dmg, float rng, float cool, int cnt, float slowMul, float durMul)> skillBonuses = new();
    private HashSet<string> activeSpecialties = new HashSet<string>();

    #region Properties(장비 + 기본 스탯 계산)
    // 기본값 + (레벨업 증가분 * 레벨) + 장비 합산
    public float MaxHealth => data.maxHealth + GetEquipSum(item => item.maxHealth);
    public float MoveSpeed => data.moveSpeed + GetEquipSum(item => item.moveSpeed);
    public float PhysicalDamage => data.physicalDamage + GetEquipSum(item => item.physicalDamage);
    public float MagicDamage => data.magicDamage + GetEquipSum(item => item.magicDamage);
    public float PhysicalDefense => data.physicalDefense + GetEquipSum(item => item.physicalDefense);
    public float DefenseRate => data.defenseRate + GetEquipSum(item => item.defense);
    public float CriticalChance => data.criticalChance + GetEquipSum(item => item.criticalChance);
    public float CriticalDamage => data.criticalDamage + GetEquipSum(item => item.criticalDamage);
    public float AttackCooldown => data.attackcooldown + GetEquipSum(item => item.moveSpeed); // 예시로 moveSpeed를 공격 쿨타임에 영향 주는 장비 스탯으로 사용

    #endregion
    void Awake()
    {
        if (Instance == null) Instance = this;
        currentHealth = data.maxHealth;
    }
    public bool HasSpecialty(string specialtyTag) => activeSpecialties.Contains(specialtyTag);

    public bool HasAnySpecialty(params string[] specialtyTags)
    {
        if (specialtyTags == null) return false;
        for (int i = 0; i < specialtyTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(specialtyTags[i]) && activeSpecialties.Contains(specialtyTags[i]))
                return true;
        }
        return false;
    }

    public bool HasAnySpecialtyContains(params string[] keywordTags)
    {
        if (keywordTags == null || keywordTags.Length == 0) return false;
        foreach (string active in activeSpecialties)
        {
            if (string.IsNullOrEmpty(active)) continue;
            for (int i = 0; i < keywordTags.Length; i++)
            {
                if (!string.IsNullOrEmpty(keywordTags[i]) && active.Contains(keywordTags[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }
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

        var b = skillBonuses[skillData];
        Debug.Log($"<color=cyan>[{skillData.skillName} 강화]</color> " +
                  $"Dmg: x{b.dmg:F1}, Rng: x{b.rng:F1}, Cool: x{b.cool:F1}, Count: +{b.cnt}, " +
                  $"Slow: x{b.slowMul:F2}, Duration: x{b.durMul:F2}");
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
            if(item != null) sum += statSelector(item);
        }
        return sum;
    }
    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;
        equippedItems[newItem.slot] = newItem;

        // 체력 아이템 장착 시 현재 체력 비율 유지 혹은 보정
        if (currentHealth > MaxHealth) currentHealth = MaxHealth;

        Debug.Log($"{newItem.itemName} 장착 완료. 현재 공격력: {PhysicalDamage}");
    }

    public void Unequip(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            equippedItems.Remove(slot);
            Debug.Log($"{slot} 슬롯 장착 해제");
        }
    }
    public void TakeExp(float exp)
    {
        currentExp += exp;
        Debug.Log(currentExp);
        if (currentExp >= 2)
        {
            LevelUp();
        }
    }
    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;
        Debug.Log("Level++" + currentLevel);
        OnLevelUp?.Invoke();
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            isDead = true;
            Die();
        }
    }
    
    public void TakeFixedDamage(float damage) {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            isDead = true;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("사망");
        OnPlayerDied?.Invoke();
        GetComponent<PlayerAnimation>().PlayDie(); //사망 애니메이션
    }


}