using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 창고(Stash) — 베이스의 영구 저장 인벤토리.
/// 인벤토리와 같은 InventoryItem을 보관. DontDestroyOnLoad로 씬 유지.
/// </summary>
public class Stash : MonoBehaviour
{
    public static Stash Instance { get; private set; }
    public static event Action OnStashChanged;

    private List<InventoryItem> items = new List<InventoryItem>();
    public IReadOnlyList<InventoryItem> Items => items;

    public int Capacity = 50;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RestoreFromPersistent();
    }

    public bool AddItem(InventoryItem item)
    {
        if (item == null) return false;
        if (items.Count >= Capacity)
        {
            Debug.Log("[Stash] 창고가 가득 찼습니다.");
            return false;
        }
        items.Add(item);
        SaveToPersistent();
        OnStashChanged?.Invoke();
        Debug.Log($"[Stash] 입고: {item.itemName}");
        return true;
    }

    public void RemoveItem(InventoryItem item)
    {
        items.Remove(item);
        SaveToPersistent();
        OnStashChanged?.Invoke();
    }

    /// <summary>인벤토리 → 창고</summary>
    public bool TransferFromInventory(InventoryItem item)
    {
        if (item == null || Inventory.Instance == null) return false;
        if (!AddItem(item)) return false;
        Inventory.Instance.RemoveItem(item);
        return true;
    }

    /// <summary>창고 → 인벤토리</summary>
    public bool TransferToInventory(InventoryItem item)
    {
        if (item == null || Inventory.Instance == null) return false;
        Inventory.Instance.AddItem(item);
        RemoveItem(item);
        return true;
    }

    void SaveToPersistent()
    {
        if (GameDataManager.Instance == null) return;
        GameDataManager.Instance.SaveStash(items);
    }

    void RestoreFromPersistent()
    {
        if (GameDataManager.Instance == null) return;
        var saved = GameDataManager.Instance.LoadStash();
        items.Clear();
        if (saved != null) items.AddRange(saved);
        OnStashChanged?.Invoke();
    }
}
