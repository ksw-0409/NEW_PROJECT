using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptable Objects/PersistentData")]
public class PersistentData : ScriptableObject
{
    [Header("재화")]
    public int gold = 1000;
    public int normalCurrency = 10;  // 일반 옵션 강화 재화
    public int specialCurrency = 10; // 특수 옵션 강화 재화

    [Header("장비")]
    public List<string> equippedItems = new List<string>();

    [Header("진행")]
    public int currentFloor = 1;

    [Header("런타임 아이템 데이터 (리롤 결과 저장)")]
    public List<RuntimeItemData> runtimeItems = new List<RuntimeItemData>();

    [Header("활성화된 스킬 노드 (skillName 기준)")]
    public List<string> unlockedSkillNodes = new List<string>();

    [Header("장착된 스킬 (skillName 기준)")]
    public List<string> savedSkills = new List<string>();

    public void ResetAll()
    {
        gold = 1000;
        normalCurrency = 10;
        specialCurrency = 10;
        equippedItems.Clear();
        currentFloor = 1;
        runtimeItems.Clear();
        unlockedSkillNodes.Clear();
        savedSkills.Clear();
    }
}