using UnityEngine;

[CreateAssetMenu(menuName = "Items/EquipmentData")]
public class EquipmentData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public ItemType itemType;
    public EquipmentSlot slot;
    public ItemGrade grade;
    public Sprite icon;

    [Header("Base Stats")]
    public float basePhysicalDamage;
    public float baseMagicDamage;
    public float baseCriticalChance;
    public float baseCriticalDamage;
    public float baseMaxHealth;
    public float basePhysicalDefense;
    public float baseMoveSpeed;
    public float baseAttackcooldown;

    [Header("Stats")]
    public float physicalDamage;
    public float magicDamage;
    public float criticalChance;     // 0 ~ 1 (예: 0.1f = 10%)
    public float criticalDamage;     // 기본 1.5f 등
    public float maxHealth;
    public float defense;
    public float moveSpeed;
    public float physicalDefense;    // 방어 수치 (예: 20 = 20 감소)
    public float attackcooldown;     // 공격 쿨다운 감소 수치 (예: 0.2f = 20% 감소)

    [Header("Special")]
    public int ability;              // 특수 능력 ID (0 = 없음)
}
