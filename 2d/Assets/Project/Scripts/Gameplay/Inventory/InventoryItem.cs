using System;

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

    public ItemGrade Grade => (ItemGrade)gradeInt;
}