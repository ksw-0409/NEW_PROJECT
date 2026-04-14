using System;

// 역할: 런타임에 리롤된 아이템 스탯을 저장하는 클래스

[Serializable]
public class RuntimeItemData
{
    public string itemName;
    public int itemTypeInt;
    public int slotInt;
    public int gradeInt;

    public float physicalDamage;
    public float magicDamage;
    public float criticalChance;
    public float criticalDamage;
    public float maxHealth;
    public float physicalDefense;
    public float moveSpeed;
    public float attackcooldown;

    // EquipmentData -> RuntimeItemData 변환
    public static RuntimeItemData FromEquipmentData(EquipmentData data)
    {
        return new RuntimeItemData
        {
            itemName = data.itemName,
            itemTypeInt = (int)data.itemType,
            slotInt = (int)data.slot,
            gradeInt = (int)data.grade,
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

    // RuntimeItemData -> EquipmentData 덮어씌우기
    public void ApplyTo(EquipmentData data)
    {
        data.itemName = itemName;
        data.itemType = (ItemType)itemTypeInt;
        data.slot = (EquipmentSlot)slotInt;
        data.grade = (ItemGrade)gradeInt;
        data.physicalDamage = physicalDamage;
        data.magicDamage = magicDamage;
        data.criticalChance = criticalChance;
        data.criticalDamage = criticalDamage;
        data.maxHealth = maxHealth;
        data.physicalDefense = physicalDefense;
        data.moveSpeed = moveSpeed;
        data.attackcooldown = attackcooldown;
    }
}
