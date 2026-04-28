using System;
using System.Collections.Generic;
using UnityEngine;

// 역할: 인벤토리 아이템 목록 관리
// GameDataManager를 통해 씬 전환 후에도 유지

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public static event Action OnInventoryChanged;

    private List<InventoryItem> items = new List<InventoryItem>();

    public IReadOnlyList<InventoryItem> Items => items;

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

    // 아이템 추가 (던전에서 획득 시 미감정 상태로)
    public void AddItem(EquipmentData data)
    {
        if (data == null) return;

        InventoryItem item = InventoryItem.FromEquipmentData(data, identified: false);
        items.Add(item);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] 아이템 획득: {item.itemName} ({item.Grade})");
    }

    // 아이템 제거
    public void RemoveItem(InventoryItem item)
    {
        items.Remove(item);
        OnInventoryChanged?.Invoke();
    }

    // 게임오버 시 절반 랜덤 삭제
    public void ApplyDeathPenalty()
    {
        if (items.Count == 0) return;

        int removeCount = Mathf.CeilToInt(items.Count * 0.5f);

        // Fisher-Yates 셔플로 랜덤 인덱스 선택
        List<int> indices = new List<int>(items.Count);
        for (int i = 0; i < items.Count; i++) indices.Add(i);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        // 높은 인덱스부터 제거 (인덱스 밀림 방지)
        List<int> toRemove = indices.GetRange(0, removeCount);
        toRemove.Sort((a, b) => b.CompareTo(a));
        foreach (int idx in toRemove)
            items.RemoveAt(idx);

        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] 사망 패널티 — {removeCount}개 아이템 삭제");
    }

    // 인벤토리 전체 초기화
    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}