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

    // ✨ 동적 옵션 매핑 (실제 값이 있는 옵션만)
    // key: 스탯 이름, value: 잠금 여부
    [NonSerialized] public List<StatOption> options = new List<StatOption>();

    /// <summary>아이템 선택 시 실제 옵션 목록 생성</summary>
    public void BuildOptions()
    {
        options = new List<StatOption>();
        if (physicalDamage > 0) options.Add(new StatOption("physicalDamage", $"물리 공격력: {physicalDamage:F1}"));
        if (magicDamage > 0) options.Add(new StatOption("magicDamage", $"마법 공격력: {magicDamage:F1}"));
        if (criticalChance > 0) options.Add(new StatOption("criticalChance", $"치명타 확률: {criticalChance * 100f:F1}%"));
        if (criticalDamage > 0) options.Add(new StatOption("criticalDamage", $"치명타 피해: {criticalDamage:F2}배"));
        if (maxHealth > 0) options.Add(new StatOption("maxHealth", $"최대 체력: {maxHealth:F0}"));
        if (physicalDefense > 0) options.Add(new StatOption("physicalDefense", $"방어력: {physicalDefense:F1}"));
        if (moveSpeed > 0) options.Add(new StatOption("moveSpeed", $"이동속도: {moveSpeed:F2}"));
        if (attackcooldown > 0) options.Add(new StatOption("attackcooldown", $"쿨타임 감소: {attackcooldown:F2}배"));
    }

    /// <summary>강화 후 옵션 텍스트만 갱신 (잠금 상태 유지)</summary>
    public void RefreshOptionTexts()
    {
        foreach (var opt in options)
        {
            switch (opt.statName)
            {
                case "physicalDamage": opt.displayText = $"물리 공격력: {physicalDamage:F1}"; break;
                case "magicDamage": opt.displayText = $"마법 공격력: {magicDamage:F1}"; break;
                case "criticalChance": opt.displayText = $"치명타 확률: {criticalChance * 100f:F1}%"; break;
                case "criticalDamage": opt.displayText = $"치명타 피해: {criticalDamage:F2}배"; break;
                case "maxHealth": opt.displayText = $"최대 체력: {maxHealth:F0}"; break;
                case "physicalDefense": opt.displayText = $"방어력: {physicalDefense:F1}"; break;
                case "moveSpeed": opt.displayText = $"이동속도: {moveSpeed:F2}"; break;
                case "attackcooldown": opt.displayText = $"쿨타임 감소: {attackcooldown:F2}배"; break;
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
        return data;
    }

    public ItemGrade Grade => (ItemGrade)gradeInt;
}

/// <summary>옵션 하나의 데이터 (스탯 이름 + 표시 텍스트 + 잠금 여부)</summary>
[Serializable]
public class StatOption
{
    public string statName;    // 스탯 식별자
    public string displayText; // UI 표시 텍스트
    public bool isLocked;

    public StatOption(string statName, string displayText)
    {
        this.statName = statName;
        this.displayText = displayText;
        this.isLocked = false;
    }
}