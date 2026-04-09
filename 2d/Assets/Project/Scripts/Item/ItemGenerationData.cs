using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemGenRule")]
public class ItemGenRule : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public EquipmentSlot slot;
    public ItemGrade grade;

    [Header("Physical Damage Range")]
    public float minPhys;
    public float maxPhys;

    [Header("Magic Damage Range")]
    public float minMagic;
    public float maxMagic;

    [Header("Critical Range (%)")]
    public float minCritChance; // ¿¹: 0.01 (1%)
    public float maxCritChance;

    [Header("Crit Damage Range (%)")]
    public float minCritDmg;
    public float maxCritDmg;

    [Header("Health & Defense Range")]
    public float minHealth;
    public float maxHealth;
    public float minDef;
    public float maxDef;

    [Header("Move Speed Range")]
    public float minMoveSpeed;
    public float maxMoveSpeed;
}