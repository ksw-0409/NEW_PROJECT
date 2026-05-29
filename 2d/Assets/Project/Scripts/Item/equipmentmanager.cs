using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public class IconEntry
    {
        public string iconName;   // CSV IconName 컬럼과 매칭 (예: "fa735")
        public Sprite sprite;
    }

    [Header("Database CSV")]
    public TextAsset itemDatabaseCsv;

    [Header("Icon Mapping (Inspector or Auto-populate)")]
    [Tooltip("CSV의 IconName과 매칭되는 스프라이트들. 비어 있으면 에디터에서 자동 채움.")]
    public List<IconEntry> iconEntries = new List<IconEntry>();

    public static EquipmentManager Instance { get; private set; }

    private Dictionary<int, ItemDataRow> itemDatabase = new Dictionary<int, ItemDataRow>();
    private Dictionary<string, Sprite> iconLookup;

    void Awake()
    {
        // 이미 살아있는 인스턴스가 있으면(base 씬의 비활성 AnvilCanvas 등) 자기 자신 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Bootstrap에서 만들어진 EquipmentManager가 모든 씬에 살아남도록
        DontDestroyOnLoad(gameObject);

        BuildIconLookup();
        LoadDatabase();
    }

    private void BuildIconLookup()
    {
        iconLookup = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
        if (iconEntries == null) return;
        foreach (var e in iconEntries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.iconName) || e.sprite == null) continue;
            iconLookup[e.iconName.Trim()] = e.sprite;
        }
        Debug.Log($"[EquipmentManager] 아이콘 매핑 로드: {iconLookup.Count}개");
    }

    void LoadDatabase()
    {
        if (itemDatabaseCsv == null)
        {
            Debug.LogError("[EquipmentManager] itemDatabaseCsv가 할당되지 않음!");
            return;
        }

        string[] lines = itemDatabaseCsv.text.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = lines[i].Split(new char[] { ',', '\t' });

            if (cols.Length < 28)
            {
                Debug.LogWarning($"[EquipmentManager] {i}번째 줄 컬럼 수 부족({cols.Length}): {lines[i]}");
                continue;
            }

            try
            {
                ItemDataRow row = new ItemDataRow();

                row.id = int.Parse(cols[0].Trim());
                row.itemName = cols[1].Trim();
                ParseTypeAndSlot(cols[2].Trim(), out var itType, out var itSlot);
                row.itemType = itType;
                row.slot = itSlot;
                row.grade = ParseGrade(cols[3].Trim());
                row.iconName = cols[4].Trim();

                row.basePhys       = ParseFloat(cols[5]);
                row.baseMagic      = ParseFloat(cols[6]);
                row.baseCrit       = ParseFloat(cols[7]);
                row.baseCritDmg    = ParseFloat(cols[8]);
                row.baseHealth     = ParseFloat(cols[9]);
                row.baseDef        = ParseFloat(cols[10]);
                row.baseSpeed      = ParseFloat(cols[11]);
                row.attackCooldown = ParseFloat(cols[12]);

                row.minAddPhys     = ParseFloat(cols[13]);
                row.maxAddPhys     = ParseFloat(cols[14]);
                row.minAddMagic    = ParseFloat(cols[15]);
                row.maxAddMagic    = ParseFloat(cols[16]);
                row.minAddCrit     = ParseFloat(cols[17]);
                row.maxAddCrit     = ParseFloat(cols[18]);
                row.minAddCritDmg  = ParseFloat(cols[19]);
                row.maxAddCritDmg  = ParseFloat(cols[20]);
                row.minAddHealth   = ParseFloat(cols[21]);
                row.maxAddHealth   = ParseFloat(cols[22]);
                row.minAddDef      = ParseFloat(cols[23]);
                row.maxAddDef      = ParseFloat(cols[24]);
                row.minAddSpeed    = ParseFloat(cols[25]);
                row.maxAddSpeed    = ParseFloat(cols[26]);

                row.ability = cols.Length > 27 ? int.Parse(cols[27].Trim()) : 0;

                itemDatabase[row.id] = row;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EquipmentManager] {i}번째 줄 파싱 실패: {e.Message} | 원본: {lines[i]}");
            }
        }

        Debug.Log($"[EquipmentManager] 아이템 DB 로드 완료 — 총 {itemDatabase.Count}개");
    }

    // CSV Type 컬럼(부위) → ItemType + EquipmentSlot 동시 결정
    private void ParseTypeAndSlot(string s, out ItemType type, out EquipmentSlot slot)
    {
        switch (s.Trim().ToUpperInvariant())
        {
            case "WEAPON": type = ItemType.Weapon; slot = EquipmentSlot.Weapon; return;
            case "HELMET": type = ItemType.Armor;  slot = EquipmentSlot.Helmet; return;
            case "ARMOR":  type = ItemType.Armor;  slot = EquipmentSlot.Armor;  return;
            case "PANTS":  type = ItemType.Armor;  slot = EquipmentSlot.Pants;  return;
            case "SHOES":  type = ItemType.Armor;  slot = EquipmentSlot.Shoes;  return;
            default:
                Debug.LogWarning($"[EquipmentManager] 알 수 없는 Type '{s}' → Weapon 처리");
                type = ItemType.Weapon; slot = EquipmentSlot.Weapon; return;
        }
    }

    // CSV Grade 문자열 → ItemGrade (LEGEND 같은 별칭 처리)
    private ItemGrade ParseGrade(string s)
    {
        switch (s.Trim().ToUpperInvariant())
        {
            case "COMMON":    return ItemGrade.Common;
            case "RARE":      return ItemGrade.Rare;
            case "EPIC":      return ItemGrade.Epic;
            case "UNIQUE":    return ItemGrade.Unique;
            case "LEGEND":
            case "LEGENDARY": return ItemGrade.Legendary;
            default:
                Debug.LogWarning($"[EquipmentManager] 알 수 없는 Grade '{s}' → Common 처리");
                return ItemGrade.Common;
        }
    }

    private float ParseFloat(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0f;
        return float.Parse(s.Trim(), System.Globalization.CultureInfo.InvariantCulture);
    }

    // 특정 등급의 모든 아이템 ID 목록 반환
    public System.Collections.Generic.List<int> GetIdsByGrade(ItemGrade grade)
    {
        var list = new System.Collections.Generic.List<int>();
        foreach (var kv in itemDatabase)
            if (kv.Value.grade == grade) list.Add(kv.Key);
        return list;
    }

    // 특정 등급에서 랜덤 아이템 ID 하나 반환 (없으면 -1)
    public int GetRandomIdByGrade(ItemGrade grade)
    {
        var list = GetIdsByGrade(grade);
        if (list.Count == 0) return -1;
        return list[Random.Range(0, list.Count)];
    }

    // DB에 로드된 전체 아이템 수 (디버그용)
    public int DatabaseCount => itemDatabase.Count;

    public EquipmentData CreateItem(int id)
    {
        if (!itemDatabase.ContainsKey(id))
        {
            Debug.LogError($"ID {id}을(를) 데이터베이스에서 찾을 수 없습니다.");
            return null;
        }

        ItemDataRow data = itemDatabase[id];
        EquipmentData newItem = ScriptableObject.CreateInstance<EquipmentData>();

        newItem.itemName = data.itemName;
        newItem.itemType = data.itemType;
        newItem.grade = data.grade;
        newItem.slot = data.slot;

        newItem.physicalDamage  = data.basePhys    + Random.Range(data.minAddPhys, data.maxAddPhys);
        newItem.magicDamage     = data.baseMagic   + Random.Range(data.minAddMagic, data.maxAddMagic);
        newItem.criticalChance  = data.baseCrit    + Random.Range(data.minAddCrit, data.maxAddCrit);
        newItem.criticalDamage  = data.baseCritDmg + Random.Range(data.minAddCritDmg, data.maxAddCritDmg);
        newItem.maxHealth       = data.baseHealth  + Random.Range(data.minAddHealth, data.maxAddHealth);
        newItem.physicalDefense = data.baseDef     + Random.Range(data.minAddDef, data.maxAddDef);
        newItem.moveSpeed       = data.baseSpeed   + Random.Range(data.minAddSpeed, data.maxAddSpeed);
        newItem.attackcooldown  = data.attackCooldown;
        newItem.ability         = data.ability;

        // 아이콘 — 인스펙터에서 매핑된 dictionary로 먼저 시도, 폴백으로 Resources
        Sprite icon = null;
        if (iconLookup != null && iconLookup.TryGetValue(data.iconName, out var found))
        {
            icon = found;
        }
        else
        {
            icon = Resources.Load<Sprite>("Icons/" + data.iconName);
        }

        if (icon == null)
        {
            Debug.LogWarning($"[EquipmentManager] 아이콘을 찾을 수 없음: '{data.iconName}' (ID={id}, 이름={data.itemName})");
        }
        newItem.icon = icon;

        return newItem;
    }
}