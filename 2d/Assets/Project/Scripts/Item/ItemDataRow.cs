[System.Serializable]
public class ItemDataRow
{
    public int id;
    public string itemName;
    public ItemType itemType;
    public EquipmentSlot slot;
    public ItemGrade grade;
    public string iconName;

    // 기본 능력치
    public float basePhys, baseMagic, baseCrit, baseCritDmg, baseHealth, baseDef, baseSpeed, attackCooldown;

    // 추가 옵션 능력치 범위
    public float minAddPhys, maxAddPhys;
    public float minAddMagic, maxAddMagic;
    public float minAddCrit, maxAddCrit;
    public float minAddCritDmg, maxAddCritDmg;
    public float minAddHealth, maxAddHealth;
    public float minAddDef, maxAddDef;
    public float minAddSpeed, maxAddSpeed;

    // 특수 능력 ID (0 = 없음)
    public int ability;
}
