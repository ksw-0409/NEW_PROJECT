using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[CreateAssetMenu(menuName = "Items/EquipmentData")]
public class EquipmentData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public ItemType itemType;
    public EquipmentSlot slot;
    public ItemGrade grade;
    public Sprite icon;

    [Header("Stats")]
    public float physicalDamage;
    public float magicDamage;
    public float criticalChance; // 0 ~ 1 (예: 0.1f = 10%)
    public float criticalDamage; // 기본 1.5배 등
    public float maxHealth;
    public float defense;
    public float moveSpeed;
    public float physicalDefense; // 방어력 수치 (예: 20 = 20 방어력)
    public float attackcooldown; // 공격 쿨타임 감소 수치 (예: 0.2f = 20% 감소)
}