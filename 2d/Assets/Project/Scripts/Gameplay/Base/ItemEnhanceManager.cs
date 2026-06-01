using UnityEngine;
using System;

// 역할: 아이템 강화 핵심 로직 (옵션 리롤 + 골드 차감)

public class ItemEnhanceManager : MonoBehaviour
{
    [Header("강화 비용")]
    [SerializeField] private int baseCost = 10;
    [SerializeField] private int lockCostPerOption = 50; // 잠금 옵션당 추가 비용

    public event Action<InventoryItem> OnEnhanceSuccess;
    public event Action<string> OnEnhanceFailed;

    // 잠긴 옵션 수에 따라 비용 계산
    public int GetEnhanceCost(InventoryItem item = null)
    {
        if (item == null || item.options == null) return baseCost;
        int lockedCount = 0;
        foreach (var opt in item.options)
            if (opt.isLocked) lockedCount++;
        return baseCost + lockedCount * lockCostPerOption;
    }

    public void TryEnhance(InventoryItem target)
    {
        if (target == null)
        {
            OnEnhanceFailed?.Invoke("강화할 아이템이 선택되지 않았습니다.");
            return;
        }

        if (!target.isIdentified)
        {
            OnEnhanceFailed?.Invoke("감정되지 않은 아이템은 강화할 수 없습니다.");
            return;
        }

        int cost = GetEnhanceCost(target);
        if (!GameDataManager.Instance.SpendGold(cost))
        {
            OnEnhanceFailed?.Invoke($"골드가 부족합니다. (필요: {cost}G)");
            return;
        }

        ItemStatRoller.RollStats(target);
        OnEnhanceSuccess?.Invoke(target);
    }
}