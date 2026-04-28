using UnityEngine;
using System;

// 역할: 아이템 감정 핵심 로직 (골드 차감 + 랜덤 스탯 부여)

public class ItemIdentifyManager : MonoBehaviour
{
    [Header("감정 비용")]
    [SerializeField] private int identifyCost = 10;

    public event Action<InventoryItem> OnIdentifySuccess;
    public event Action<string> OnIdentifyFailed;

    public void TryIdentify(InventoryItem target)
    {
        if (target == null)
        {
            OnIdentifyFailed?.Invoke("감정할 아이템이 선택되지 않았습니다.");
            return;
        }

        if (target.isIdentified)
        {
            OnIdentifyFailed?.Invoke("이미 감정된 아이템입니다.");
            return;
        }

        if (!GameDataManager.Instance.SpendGold(identifyCost))
        {
            OnIdentifyFailed?.Invoke($"골드가 부족합니다. (필요: {identifyCost}G)");
            return;
        }

        // 감정 시 랜덤 스탯 부여
        ItemStatRoller.RollStats(target);
        target.isIdentified = true;

        OnIdentifySuccess?.Invoke(target);
    }

    public int GetIdentifyCost() => identifyCost;
}