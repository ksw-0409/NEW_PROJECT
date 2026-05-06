using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    public PlayerData data;
    public event Action OnLevelUp;
    public static PlayerStats Instance;

    public float currentHealth;
    public float currentLevel = 1;
    public float currentExp = 0;

    // 스킬 노드 보너스: dmg/rng/cool 곱, cnt 합, slowMul(둔화 강도)·durMul(지속시간) 곱
    private Dictionary<SkillData, (float dmg, float rng, float cool, int cnt, float slowMul, float durMul)> skillBonuses = new();
    private HashSet<string> activeSpecialties = new HashSet<string>();

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

    // --- 이하 장비 및 기존 로직 (동일) ---
    private Dictionary<EquipmentSlot, EquipmentData> equippedItems = new Dictionary<EquipmentSlot, EquipmentData>();

    #region Properties
    public float MaxHealth => data.maxHealth + GetEquipSum(item => item.maxHealth);
    public float MoveSpeed => (data.moveSpeed + GetEquipSum(item => item.moveSpeed)) * (1f + tempSpeedBonus);
    private float tempSpeedBonus = 0f;
    public void AddTemporarySpeedBonus(float delta) { tempSpeedBonus += delta; }
    public float PhysicalDamage => data.physicalDamage + GetEquipSum(item => item.physicalDamage);
    public float MagicDamage => data.magicDamage + GetEquipSum(item => item.magicDamage);
    public float PhysicalDefense => data.physicalDefense + GetEquipSum(item => item.physicalDefense);
    public float DefenseRate => data.defenseRate + GetEquipSum(item => item.defense);
    public float CriticalChance => data.criticalChance + GetEquipSum(item => item.criticalChance);
    public float CriticalDamage => data.criticalDamage + GetEquipSum(item => item.criticalDamage);
    public float AttackCooldown => data.attackcooldown + GetEquipSum(item => item.moveSpeed);
    #endregion

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
        Debug.Log($"{newItem.itemName} 장착 완료.");
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
        if (currentExp >= 2) LevelUp();
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;
        OnLevelUp?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) Die();
    }

    private void Die()
    {
        Debug.Log("사망");
        GetComponent<PlayerAnimation>()?.PlayDie();
    }
}