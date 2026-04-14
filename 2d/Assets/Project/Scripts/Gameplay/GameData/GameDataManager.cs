using System;
using System.Collections.Generic;
using UnityEngine;

// 역할: 씬 전환에도 살아남는 유일한 데이터 관리자
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public static event Action<int> OnGoldChanged;
    public static event Action<int> OnNormalCurrencyChanged;
    public static event Action<int> OnSpecialCurrencyChanged;
    public static event Action<List<string>> OnItemsChanged;
    public static event Action<int> OnFloorChanged;

    [SerializeField] private PersistentData persistentData;

    public int Gold => persistentData.gold;
    public int NormalCurrency => persistentData.normalCurrency;
    public int SpecialCurrency => persistentData.specialCurrency;
    public IReadOnlyList<string> Items => persistentData.equippedItems;
    public int CurrentFloor => persistentData.currentFloor;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        persistentData.ResetAll();
        Debug.Log($"[GameDataManager] ResetAll 호출 — runtimeItems 개수: {persistentData.runtimeItems.Count}");
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        persistentData.gold += amount;
        OnGoldChanged?.Invoke(persistentData.gold);
    }

    public bool SpendGold(int amount)
    {
        if (persistentData.gold < amount) return false;
        persistentData.gold -= amount;
        OnGoldChanged?.Invoke(persistentData.gold);
        return true;
    }

    public void AddNormalCurrency(int amount)
    {
        if (amount <= 0) return;
        persistentData.normalCurrency += amount;
        OnNormalCurrencyChanged?.Invoke(persistentData.normalCurrency);
    }

    public bool SpendNormalCurrency(int amount)
    {
        if (persistentData.normalCurrency < amount) return false;
        persistentData.normalCurrency -= amount;
        OnNormalCurrencyChanged?.Invoke(persistentData.normalCurrency);
        return true;
    }

    public void SaveUnlockedNode(string skillName)
    {
        if (!persistentData.unlockedSkillNodes.Contains(skillName))
            persistentData.unlockedSkillNodes.Add(skillName);
    }

    public bool IsNodeUnlocked(string skillName)
    {
        return persistentData.unlockedSkillNodes.Contains(skillName);
    }

    public void AddSpecialCurrency(int amount)
    {
        if (amount <= 0) return;
        persistentData.specialCurrency += amount;
        OnSpecialCurrencyChanged?.Invoke(persistentData.specialCurrency);
    }

    public bool SpendSpecialCurrency(int amount)
    {
        if (persistentData.specialCurrency < amount) return false;
        persistentData.specialCurrency -= amount;
        OnSpecialCurrencyChanged?.Invoke(persistentData.specialCurrency);
        return true;
    }

    public void AddItem(string itemId)
    {
        persistentData.equippedItems.Add(itemId);
        OnItemsChanged?.Invoke(persistentData.equippedItems);
    }

    public void RemoveItem(string itemId)
    {
        persistentData.equippedItems.Remove(itemId);
        OnItemsChanged?.Invoke(persistentData.equippedItems);
    }

    public void SetFloor(int floor)
    {
        persistentData.currentFloor = floor;
        OnFloorChanged?.Invoke(floor);
        Debug.Log($"[GameDataManager] 현재 층: {floor}");
    }

    public void NextFloor()
    {
        SetFloor(persistentData.currentFloor + 1);
    }

    public void ResetFloor()
    {
        SetFloor(1);
    }

    // 리롤된 아이템 저장 (슬롯 인덱스 기준)
    public void SaveRuntimeItem(int slotIndex, EquipmentData data)
    {
        var list = persistentData.runtimeItems;

        while (list.Count <= slotIndex)
            list.Add(null);

        list[slotIndex] = RuntimeItemData.FromEquipmentData(data);
        Debug.Log($"[GameDataManager] 슬롯 {slotIndex} 아이템 저장: {data.itemName}");
    }

    // 저장된 아이템을 EquipmentData에 덮어씌우기
    public void LoadRuntimeItem(int slotIndex, EquipmentData target)
    {
        var list = persistentData.runtimeItems;

        if (slotIndex >= list.Count || list[slotIndex] == null) return;

        list[slotIndex].ApplyTo(target);
        Debug.Log($"[GameDataManager] 슬롯 {slotIndex} 아이템 불러오기: {target.itemName}");
    }

    // 해당 슬롯에 저장된 리롤 데이터가 있는지 확인
    public bool HasRuntimeItem(int slotIndex)
    {
        var list = persistentData.runtimeItems;
        return slotIndex < list.Count && list[slotIndex] != null;
    }

    public void ApplyGameOverPenalty()
    {
        ApplyGoldPenalty();
        ApplyItemPenalty();
        ResetFloor();
    }

    private void ApplyGoldPenalty()
    {
        persistentData.gold = Mathf.FloorToInt(persistentData.gold * 0.5f);
        OnGoldChanged?.Invoke(persistentData.gold);
    }

    private void ApplyItemPenalty()
    {
        List<string> items = persistentData.equippedItems;
        if (items.Count == 0) return;

        int removeCount = Mathf.CeilToInt(items.Count * 0.5f);
        List<int> indices = new List<int>(items.Count);
        for (int i = 0; i < items.Count; i++) indices.Add(i);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        List<int> toRemove = indices.GetRange(0, removeCount);
        toRemove.Sort((a, b) => b.CompareTo(a));
        foreach (int idx in toRemove)
            items.RemoveAt(idx);

        OnItemsChanged?.Invoke(items);
    }

#if UNITY_EDITOR
    [ContextMenu("게임오버 패널티 테스트")]
    private void TestPenalty()
    {
        Debug.Log($"[패널티 전] 골드: {Gold}, 장비: {Items.Count}개, 층: {CurrentFloor}");
        ApplyGameOverPenalty();
        Debug.Log($"[패널티 후] 골드: {Gold}, 장비: {Items.Count}개, 층: {CurrentFloor}");
    }
#endif
}