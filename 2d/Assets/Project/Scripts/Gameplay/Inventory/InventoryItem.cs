using System;
using UnityEngine;

// 역할: 인벤토리에 저장되는 아이템 런타임 데이터
// 미감정 상태로 획득, 거점에서 감정 후 스탯 공개

[Serializable]
public class InventoryItem
{
    public string itemName;
    public int gradeInt;       // ItemGrade enum → int
    public int itemTypeInt;    // ItemType enum → int
    public int slotInt;        // EquipmentSlot enum → int
    public string iconName;    // Resources/Icons/ 기준
    public Sprite iconSprite;  // ✨ 스프라이트 직접 보관 (Resources.Load 불필요)
    public bool isIdentified;  // 감정 여부

    // 스탯 (감정 후 공개)
    public float physicalDamage;
    public float magicDamage;
    public float criticalChance;
    public float criticalDamage;
    public float maxHealth;
    public float physicalDefense;
    public float moveSpeed;
    public float attackcooldown;

    // EquipmentData → InventoryItem 변환
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

    // ✨ InventoryItem → EquipmentData 런타임 변환 (장착 시 PlayerStats에 넘기기 위해 사용)
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