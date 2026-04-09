using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public TextAsset itemDatabaseCsv;
    private Dictionary<int, ItemDataRow> itemDatabase = new Dictionary<int, ItemDataRow>();

    void Awake()
    {
        LoadDatabase();
    }

    void LoadDatabase()
    {
        if (itemDatabaseCsv == null) return;

        // \r 제거 후 줄바꿈으로 분리
        string[] lines = itemDatabaseCsv.text.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            // 핵심 수정: 쉼표(,), 탭(\t) 중 어떤 것이 있어도 잘라내고 빈 칸은 제거함
            string[] cols = lines[i].Split(new char[] { ',', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (cols.Length < 10) continue;

            try
            {
                ItemDataRow row = new ItemDataRow();

                // 모든 데이터 읽을 때 .Trim()으로 앞뒤 공백 확실히 제거
                row.id = int.Parse(cols[0].Trim());
                row.itemName = cols[1].Trim();
                row.itemType = (ItemType)System.Enum.Parse(typeof(ItemType), cols[2].Trim(), true);
                row.grade = (ItemGrade)System.Enum.Parse(typeof(ItemGrade), cols[3].Trim(), true);
                row.iconName = cols[4].Trim();

                // 숫자 변환부 (인덱스 번호가 꼬이지 않았는지 확인하세요)
                row.basePhys = float.Parse(cols[5].Trim());
                row.baseMagic = float.Parse(cols[6].Trim());
                // ... (나머지 컬럼들도 똑같이 .Trim() 붙여서 진행)

                itemDatabase.Add(row.id, row);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{i}번째 줄 파싱 오류: {e.Message} | 데이터: {lines[i]}");
            }
        }
    }

    public EquipmentData CreateItem(int id)
    {
        if (!itemDatabase.ContainsKey(id))
        {
            Debug.LogError($"ID {id}를 데이터베이스에서 찾을 수 없습니다.");
            return null;
        }

        ItemDataRow data = itemDatabase[id];
        EquipmentData newItem = ScriptableObject.CreateInstance<EquipmentData>();

        // 최종 스탯 결정: 기본값 + 랜덤(최소 추가, 최대 추가)
        newItem.itemName = data.itemName;
        newItem.itemType = data.itemType;
        newItem.grade = data.grade;

        newItem.physicalDamage = data.basePhys + Random.Range(data.minAddPhys, data.maxAddPhys);
        newItem.magicDamage = data.baseMagic + Random.Range(data.minAddMagic, data.maxAddMagic);
        newItem.criticalChance = data.baseCrit + Random.Range(data.minAddCrit, data.maxAddCrit);
        newItem.criticalDamage = data.baseCritDmg + Random.Range(data.minAddCritDmg, data.maxAddCritDmg);
        newItem.maxHealth = data.baseHealth + Random.Range(data.minAddHealth, data.maxAddHealth);
        newItem.physicalDefense = data.baseDef + Random.Range(data.minAddDef, data.maxAddDef);
        newItem.moveSpeed = data.baseSpeed + Random.Range(data.minAddSpeed, data.maxAddSpeed);

        // 아이콘 로드 (Resources/Icons 폴더 기준)
        newItem.icon = Resources.Load<Sprite>("Icons/" + data.iconName);

        return newItem;
    }
}