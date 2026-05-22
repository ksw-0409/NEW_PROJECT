using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 역할: 장비창 컨트롤러 — 슬롯 5개(무기/투구/갑옷/장갑/신발) + 하단 스탯 패널
// InventoryUI와 함께 Tab키로 열리고 닫힘 (InventoryUI가 SetActive 제어)

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

    // 현재 장착 아이템 (슬롯별)
    private Dictionary<EquipmentSlot, InventoryItem> equippedItems
        = new Dictionary<EquipmentSlot, InventoryItem>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void OnEnable()
    {
        RefreshAllSlots();
        RefreshStatPanel();
    }

    // ─── 장착 ─────────────────────────────────────────────────────────
    /// <summary>
    /// 인벤토리 슬롯 클릭 시 InventorySlot이 호출.
    /// 감정된 아이템만 장착 가능. 같은 슬롯에 이미 장착된 아이템은 인벤토리로 자동 반환.
    /// </summary>
    public void TryEquip(InventoryItem item)
    {
        if (item == null)
        {
            Debug.Log("[EquipUI] 아이템 없음");
            return;
        }
        if (!item.isIdentified)
        {
            Debug.Log("[EquipUI] 감정되지 않은 아이템은 장착할 수 없습니다.");
            return;
        }

        EquipmentSlot slot = (EquipmentSlot)item.slotInt;

        // 기존 장착 아이템 → 인벤토리 반환
        if (equippedItems.TryGetValue(slot, out var prev) && prev != null)
        {
            Inventory.Instance?.AddItem(prev);   // InventoryItem 직접 반환 (isIdentified 유지)
            Debug.Log($"[EquipUI] {prev.itemName} 해제 → 인벤토리 반환");
        }

        // 인벤토리에서 제거 후 장착
        Inventory.Instance?.RemoveItem(item);
        equippedItems[slot] = item;

        // PlayerStats 반영
        PlayerStats.Instance?.Equip(item.ToEquipmentData());

        RefreshAllSlots();
        RefreshStatPanel();

        // 인벤토리 슬롯 갱신
        FindFirstObjectByType<InventoryUI>()?.ForceRefresh();

        Debug.Log($"[EquipUI] {item.itemName} 장착 (슬롯: {EquipmentSlotUI.GetSlotKoreanName(slot)})");
    }

    // ─── 해제 ─────────────────────────────────────────────────────────
    /// <summary>우클릭으로 슬롯 해제 시 EquipmentSlotUI가 호출.</summary>
    public void UnequipSlot(EquipmentSlot slot)
    {
        if (!equippedItems.TryGetValue(slot, out var item) || item == null) return;

        if (Inventory.Instance != null && Inventory.Instance.Items.Count >= 28)
        {
            Debug.Log("[EquipUI] 인벤토리가 가득 찼습니다.");
            return;
        }

        // 인벤토리 반환
        Inventory.Instance?.AddItem(item);
        equippedItems.Remove(slot);

        // PlayerStats 해제
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
        RefreshSlot(armorSlot,  EquipmentSlot.Armor);
        RefreshSlot(pantsSlot,  EquipmentSlot.Pants);
        RefreshSlot(shoesSlot,  EquipmentSlot.Shoes);
    }

    private void RefreshSlot(EquipmentSlotUI slotUI, EquipmentSlot slot)
    {
        if (slotUI == null) return;
        equippedItems.TryGetValue(slot, out var item);
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
                $"쿨타임 감소  {PlayerStats.Instance.AttackCooldown:F2}배";
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