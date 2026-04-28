using UnityEngine;
using System;

// 역할: 아이템 강화 핵심 로직 (옵션 리롤 + 골드 차감)

public class ItemEnhanceManager : MonoBehaviour
{
    [Header("강화 비용")]
    [SerializeField] private int enhanceCost = 10;

    public event Action<InventoryItem> OnEnhanceSuccess;
    public event Action<string> OnEnhanceFailed;

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

        if (!GameDataManager.Instance.SpendGold(enhanceCost))
        {
            OnEnhanceFailed?.Invoke($"골드가 부족합니다. (필요: {enhanceCost}G)");
            return;
        }

        // 리롤
        ItemStatRoller.RollStats(target);
        OnEnhanceSuccess?.Invoke(target);
    }

    public int GetEnhanceCost() => enhanceCost;
}