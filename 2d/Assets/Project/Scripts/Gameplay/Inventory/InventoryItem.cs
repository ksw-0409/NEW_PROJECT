using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public string itemName;
    public int gradeInt;
    public int itemTypeInt;
    public int slotInt;
    public string iconName;
    public Sprite iconSprite;
    public bool isIdentified;

    public float physicalDamage;
    public float magicDamage;
    public float criticalChance;
    public float criticalDamage;
    public float maxHealth;
    public float physicalDefense;
    public float moveSpeed;
    public float attackcooldown;
    public int ability;

    // 기본 수치 (툴팁 표시용)
    public float basePhysicalDamage;
    public float baseMagicDamage;
    public float baseCriticalChance;
    public float baseCriticalDamage;
    public float baseMaxHealth;
    public float basePhysicalDefense;
    public float baseMoveSpeed;
    public float baseAttackcooldown;

    [NonSerialized] public List<StatOption> options = new List<StatOption>();

    /// <summary>아이템 선택 시 실제 옵션 목록 생성 (추가 수치만 표시)</summary>
    public void BuildOptions()
    {
        options = new List<StatOption>();
        float addPhys = physicalDamage - basePhysicalDamage;
        float addMagic = magicDamage - baseMagicDamage;
        float addCrit = criticalChance - baseCriticalChance;
        float addCritDmg = criticalDamage - baseCriticalDamage;
        float addHp = maxHealth - baseMaxHealth;
        float addDef = physicalDefense - basePhysicalDefense;
        float addSpeed = moveSpeed - baseMoveSpeed;

        if (basePhysicalDamage > 0) options.Add(new StatOption("physicalDamage", $"물리 공격력: +{addPhys:F1}"));
        if (baseMagicDamage > 0) options.Add(new StatOption("magicDamage", $"마법 공격력: +{addMagic:F1}"));
        if (baseCriticalChance > 0) options.Add(new StatOption("criticalChance", $"치명타 확률: +{addCrit:F1}%"));
        if (baseCriticalDamage > 0) options.Add(new StatOption("criticalDamage", $"치명타 피해: +{addCritDmg:F2}배"));
        if (baseMaxHealth > 0) options.Add(new StatOption("maxHealth", $"최대 체력: +{addHp:F0}"));
        if (basePhysicalDefense > 0) options.Add(new StatOption("physicalDefense", $"방어력: +{addDef:F1}"));
        if (baseMoveSpeed > 0) options.Add(new StatOption("moveSpeed", $"이동속도: +{addSpeed:F2}"));
        if (baseAttackcooldown > 0) options.Add(new StatOption("attackcooldown", $"쿨타임 감소: -{attackcooldown:F2}초"));
    }

    /// <summary>강화 후 옵션 텍스트만 갱신 (잠금 상태 유지)</summary>
    public void RefreshOptionTexts()
    {
        float addPhys = physicalDamage - basePhysicalDamage;
        float addMagic = magicDamage - baseMagicDamage;
        float addCrit = criticalChance - baseCriticalChance;
        float addCritDmg = criticalDamage - baseCriticalDamage;
        float addHp = maxHealth - baseMaxHealth;
        float addDef = physicalDefense - basePhysicalDefense;
        float addSpeed = moveSpeed - baseMoveSpeed;

        foreach (var opt in options)
        {
            switch (opt.statName)
            {
                case "physicalDamage": opt.displayText = $"물리 공격력: +{addPhys:F1}"; break;
                case "magicDamage": opt.displayText = $"마법 공격력: +{addMagic:F1}"; break;
                case "criticalChance": opt.displayText = $"치명타 확률: +{addCrit:F1}%"; break;
                case "criticalDamage": opt.displayText = $"치명타 피해: +{addCritDmg:F2}배"; break;
                case "maxHealth": opt.displayText = $"최대 체력: +{addHp:F0}"; break;
                case "physicalDefense": opt.displayText = $"방어력: +{addDef:F1}"; break;
                case "moveSpeed": opt.displayText = $"이동속도: +{addSpeed:F2}"; break;
                case "attackcooldown": opt.displayText = $"쿨타임 감소: -{attackcooldown:F2}초"; break;
            }
        }
    }

    public static InventoryItem FromEquipmentData(EquipmentData data, bool identified = false)
    {
        return new InventoryItem
        {
            itemName = data.itemName,
            gradeInt = (int)data.grade,
            itemTypeInt = (int)data.itemType,
            slotInt = (int)data.slot,
            iconName = data.icon != null ? data.icon.name : "",
            iconSprite = data.icon,
            isIdentified = identified,
            physicalDamage = data.physicalDamage,
            magicDamage = data.magicDamage,
            criticalChance = data.criticalChance,
            criticalDamage = data.criticalDamage,
            maxHealth = data.maxHealth,
            physicalDefense = data.physicalDefense,
            moveSpeed = data.moveSpeed,
            attackcooldown = data.attackcooldown,
            ability = data.ability,
            basePhysicalDamage = data.basePhysicalDamage,
            baseMagicDamage = data.baseMagicDamage,
            baseCriticalChance = data.baseCriticalChance,
            baseCriticalDamage = data.baseCriticalDamage,
            baseMaxHealth = data.baseMaxHealth,
            basePhysicalDefense = data.basePhysicalDefense,
            baseMoveSpeed = data.baseMoveSpeed,
            baseAttackcooldown = data.baseAttackcooldown,
        };
    }

    public EquipmentData ToEquipmentData()
    {
        var data = ScriptableObject.CreateInstance<EquipmentData>();
        data.itemName = itemName;
        data.grade = (ItemGrade)gradeInt;
        data.itemType = (ItemType)itemTypeInt;
        data.slot = (EquipmentSlot)slotInt;
        data.physicalDamage = physicalDamage;
        data.magicDamage = magicDamage;
        data.criticalChance = criticalChance;
        data.criticalDamage = criticalDamage;
        data.maxHealth = maxHealth;
        data.physicalDefense = physicalDefense;
        data.moveSpeed = moveSpeed;
        data.attackcooldown = attackcooldown;
        data.icon = iconSprite;
        data.ability = ability;
        data.basePhysicalDamage = basePhysicalDamage;
        data.baseMagicDamage = baseMagicDamage;
        data.baseCriticalChance = baseCriticalChance;
        data.baseCriticalDamage = baseCriticalDamage;
        data.baseMaxHealth = baseMaxHealth;
        data.basePhysicalDefense = basePhysicalDefense;
        data.baseMoveSpeed = baseMoveSpeed;
        data.baseAttackcooldown = baseAttackcooldown;
        return data;
    }

    public ItemGrade Grade => (ItemGrade)gradeInt;
}

[Serializable]
public class StatOption
{
    public string statName;
    public string displayText;
    public bool isLocked;

    public StatOption(string statName, string displayText)
    {
        this.statName = statName;
        this.displayText = displayText;
        this.isLocked = false;
    }
}