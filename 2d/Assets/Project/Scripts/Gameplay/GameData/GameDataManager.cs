using System;
using System.Collections.Generic;
using UnityEngine;

// 역할: 씬 전환에도 살아남는 유일한 데이터 관리자

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public static event Action<int> OnGoldChanged;           // 현재 골드
    public static event Action<List<string>> OnItemsChanged; // 현재 장비 목록

    
    [SerializeField] private PersistentData persistentData;

    public int Gold => persistentData.gold;
    public IReadOnlyList<string> Items => persistentData.equippedItems;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    public void ApplyGameOverPenalty()
    {
        ApplyGoldPenalty();
        ApplyItemPenalty();
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
        {
            items.RemoveAt(idx);
        }

        OnItemsChanged?.Invoke(items);
    }

#if UNITY_EDITOR
    [ContextMenu("게임오버 패널티 테스트")]
    private void TestPenalty()
    {
        Debug.Log($"[패널티 전] 골드: {Gold}, 장비: {Items.Count}개");
        ApplyGameOverPenalty();
        Debug.Log($"[패널티 후] 골드: {Gold}, 장비: {Items.Count}개");
    }
#endif
}