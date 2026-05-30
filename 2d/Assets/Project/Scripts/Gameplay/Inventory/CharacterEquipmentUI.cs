using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 역할: 장비창 컨트롤러 — 슬롯 5개(무기/투구/갑옷/하의/신발) + 하단 스탯 패널
// InventoryUI와 함께 Tab키로 열리고 닫힘
// ✨ 장착 아이템은 GameDataManager에 저장되어 씬 전환 후에도 유지

public class CharacterEquipmentUI : MonoBehaviour
{
    public static CharacterEquipmentUI Instance { get; private set; }

    [Header("장비 슬롯 (Inspector에서 슬롯 타입 맞춰 연결)")]
    [SerializeField] private EquipmentSlotUI weaponSlot;
    [SerializeField] private EquipmentSlotUI helmetSlot;
    [SerializeField] private EquipmentSlotUI armorSlot;
    [SerializeField] private EquipmentSlotUI pantsSlot;
    [SerializeField] private EquipmentSlotUI shoesSlot;

    [Header("하단 스탯 패널 (공격 / 방어)")]
    [SerializeField] private TextMeshProUGUI offensiveStatText;
    [SerializeField] private TextMeshProUGUI defensiveStatText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void OnEnable()
    {
        RestoreEquippedItems();
        RefreshAllSlots();
        RefreshStatPanel();
    }

    // ─── 복원 ──────────────────────────────────────────────────────────
    /// <summary>씬 로드 시 GameDataManager에서 장착 아이템 복원 및 PlayerStats 재적용</summary>
    private void RestoreEquippedItems()
    {
        if (GameDataManager.Instance == null) return;

        var saved = GameDataManager.Instance.GetAllEquippedItems();


        foreach (var kv in saved)
        {
            PlayerStats.Instance?.Equip(kv.Value.ToEquipmentData());
        }

        // 거점 씬이면 체력 전체 회복 (패시브/장비 보너스 반영된 MaxHealth로)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneController.SceneName.Base)
            PlayerStats.Instance?.HealToFull();
    }

    // ─── 장착 ─────────────────────────────────────────────────────────
    public void TryEquip(InventoryItem item)
    {
        if (item == null) { Debug.Log("[EquipUI] 아이템 없음"); return; }
        if (!item.isIdentified) { Debug.Log("[EquipUI] 감정되지 않은 아이템은 장착할 수 없습니다."); return; }

        EquipmentSlot slot = (EquipmentSlot)item.slotInt;

        // 기존 장착 아이템 → 인벤토리 반환
        var prev = GameDataManager.Instance?.GetEquippedItem(slot);
        if (prev != null)
        {
            Inventory.Instance?.AddItem(prev);
            Debug.Log($"[EquipUI] {prev.itemName} 해제 → 인벤토리 반환");
        }

        // 인벤토리에서 제거 후 장착
        Inventory.Instance?.RemoveItem(item);
        GameDataManager.Instance?.SetEquippedItem(slot, item);

        // PlayerStats 반영
        PlayerStats.Instance?.Equip(item.ToEquipmentData());

        RefreshAllSlots();
        RefreshStatPanel();
        FindFirstObjectByType<InventoryUI>()?.ForceRefresh();

        Debug.Log($"[EquipUI] {item.itemName} 장착 (슬롯: {EquipmentSlotUI.GetSlotKoreanName(slot)})");
    }

    // ─── 해제 ─────────────────────────────────────────────────────────
    public void UnequipSlot(EquipmentSlot slot)
    {
        var item = GameDataManager.Instance?.GetEquippedItem(slot);
        if (item == null) return;

        if (Inventory.Instance != null && Inventory.Instance.Items.Count >= 28)
        {
            Debug.Log("[EquipUI] 인벤토리가 가득 찼습니다.");
            return;
        }

        Inventory.Instance?.AddItem(item);
        GameDataManager.Instance?.RemoveEquippedItem(slot);
        PlayerStats.Instance?.Unequip(slot);

        RefreshAllSlots();
        RefreshStatPanel();
        FindFirstObjectByType<InventoryUI>()?.ForceRefresh();

        Debug.Log($"[EquipUI] {EquipmentSlotUI.GetSlotKoreanName(slot)} 슬롯 해제");
    }

    // ─── 비주얼 갱신 ──────────────────────────────────────────────────
    private void RefreshAllSlots()
    {
        RefreshSlot(weaponSlot, EquipmentSlot.Weapon);
        RefreshSlot(helmetSlot, EquipmentSlot.Helmet);
        RefreshSlot(armorSlot, EquipmentSlot.Armor);
        RefreshSlot(pantsSlot, EquipmentSlot.Pants);
        RefreshSlot(shoesSlot, EquipmentSlot.Shoes);
    }

    private void RefreshSlot(EquipmentSlotUI slotUI, EquipmentSlot slot)
    {
        if (slotUI == null) return;
        var item = GameDataManager.Instance?.GetEquippedItem(slot);
        slotUI.Refresh(item);
    }

    private void RefreshStatPanel()
    {
        if (PlayerStats.Instance == null) return;

        if (offensiveStatText != null)
        {
            offensiveStatText.text =
                $"물리 공격력  {PlayerStats.Instance.PhysicalDamage:F1}\n" +
                $"마법 공격력  {PlayerStats.Instance.MagicDamage:F1}\n" +
                $"치명타 확률  {PlayerStats.Instance.CriticalChance * 100f:F1}%\n" +
                $"치명타 피해  {PlayerStats.Instance.CriticalDamage:F2}배\n" +
                $"쿨타임 감소  -{PlayerStats.Instance.AttackCooldown:F2}초";
        }

        if (defensiveStatText != null)
        {
            defensiveStatText.text =
                $"최대 체력    {PlayerStats.Instance.MaxHealth:F0}\n" +
                $"방어력       {PlayerStats.Instance.PhysicalDefense:F1}\n" +
                $"피해 감소    {PlayerStats.Instance.DefenseRate * 100f:F1}%\n" +
                $"이동속도     {PlayerStats.Instance.MoveSpeed:F2}";
        }
    }
}