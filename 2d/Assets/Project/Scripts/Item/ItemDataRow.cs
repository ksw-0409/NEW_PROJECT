[System.Serializable]
public class ItemDataRow
{
    public int id;
    public string itemName;
    public ItemType itemType;
    public ItemGrade grade;
    public string iconName;

    // 고정 기본 능력치
    public float basePhys, baseMagic, baseCrit, baseCritDmg, baseHealth, baseDef, baseSpeed, attackCooldown;

    // 추가 랜덤 능력치 범위
    public float minAddPhys, maxAddPhys;
    public float minAddMagic, maxAddMagic;
    public float minAddCrit, maxAddCrit;
    public float minAddCritDmg, maxAddCritDmg;
    public float minAddHealth, maxAddHealth;
    public float minAddDef, maxAddDef;
    public float minAddSpeed, maxAddSpeed;
}