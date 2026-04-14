using UnityEngine;
using System;

// 역할: 아이템 강화 핵심 로직 (옵션 리롤 + 골드 차감 + 데이터 저장)

public class ItemEnhanceManager : MonoBehaviour
{
    [Header("강화 비용")]
    [SerializeField] private int enhanceCost = 10;

    public event Action<EquipmentData> OnEnhanceSuccess;
    public event Action<string> OnEnhanceFailed;

    public void TryEnhance(EquipmentData target, int slotIndex = 0)
    {
        if (target == null)
        {
            OnEnhanceFailed?.Invoke("강화할 아이템이 선택되지 않았습니다.");
            return;
        }

        if (!GameDataManager.Instance.SpendGold(enhanceCost))
        {
            OnEnhanceFailed?.Invoke($"골드가 부족합니다. (필요: {enhanceCost}G)");
            return;
        }

        RerollStats(target);

        GameDataManager.Instance.SaveRuntimeItem(slotIndex, target);

        OnEnhanceSuccess?.Invoke(target);
    }

    private void RerollStats(EquipmentData target)
    {
        float physMin, physMax, magMin, magMax, critMin, critMax,
              critDmgMin, critDmgMax, hpMin, hpMax, defMin, defMax,
              speedMin, speedMax;

        switch (target.grade)
        {
            case ItemGrade.Rare:
                physMin = 10f; physMax = 20f;
                magMin = 5f; magMax = 15f;
                critMin = 0.05f; critMax = 0.15f;
                critDmgMin = 1.5f; critDmgMax = 2.0f;
                hpMin = 20f; hpMax = 50f;
                defMin = 5f; defMax = 15f;
                speedMin = 0f; speedMax = 0.5f;
                break;

            case ItemGrade.Epic:
                physMin = 20f; physMax = 40f;
                magMin = 10f; magMax = 30f;
                critMin = 0.1f; critMax = 0.25f;
                critDmgMin = 1.8f; critDmgMax = 2.5f;
                hpMin = 40f; hpMax = 100f;
                defMin = 10f; defMax = 25f;
                speedMin = 0f; speedMax = 1f;
                break;

            case ItemGrade.Legendary:
                physMin = 40f; physMax = 80f;
                magMin = 20f; magMax = 60f;
                critMin = 0.2f; critMax = 0.4f;
                critDmgMin = 2.0f; critDmgMax = 3.0f;
                hpMin = 80f; hpMax = 200f;
                defMin = 20f; defMax = 50f;
                speedMin = 0.5f; speedMax = 2f;
                break;

            default: // Common
                physMin = 1f; physMax = 10f;
                magMin = 0f; magMax = 5f;
                critMin = 0f; critMax = 0.05f;
                critDmgMin = 1.2f; critDmgMax = 1.5f;
                hpMin = 5f; hpMax = 20f;
                defMin = 0f; defMax = 5f;
                speedMin = 0f; speedMax = 0.2f;
                break;
        }

        target.physicalDamage = UnityEngine.Random.Range(physMin, physMax);
        target.magicDamage = UnityEngine.Random.Range(magMin, magMax);
        target.criticalChance = UnityEngine.Random.Range(critMin, critMax);
        target.criticalDamage = UnityEngine.Random.Range(critDmgMin, critDmgMax);
        target.maxHealth = UnityEngine.Random.Range(hpMin, hpMax);
        target.physicalDefense = UnityEngine.Random.Range(defMin, defMax);
        target.moveSpeed = UnityEngine.Random.Range(speedMin, speedMax);
    }

    public int GetEnhanceCost() => enhanceCost;
}