using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 인벤토리 UI (Tab 키 열고 닫기, 슬롯 표시)
// 전투 중(스테이지 진행 중)에는 열 수 없음

public class InventoryUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventorySlot[] slots; // 4x7 = 28슬롯

    void OnEnable()
    {
        Inventory.OnInventoryChanged += RefreshSlots;
    }

    void OnDisable()
    {
        Inventory.OnInventoryChanged -= RefreshSlots;

        // UI 닫힐 때 IsUIOpen 초기화
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
            BaseInteractable.IsUIOpen = false;
        }
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (inventoryPanel.activeSelf)
            {
                ToggleInventory();
                return;
            }

            if (BaseInteractable.IsUIOpen) return;

            // 전투 중(스테이지 진행 중)이면 열 수 없음
            if (StageManager.IsStageActive && !StageManager.IsStageOver)
            {
                Debug.Log("[InventoryUI] 전투 중에는 인벤토리를 열 수 없습니다.");
                return;
            }

            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        bool isOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isOpen);
        BaseInteractable.IsUIOpen = isOpen;

        if (isOpen) RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (slots == null || Inventory.Instance == null) return;

        var items = Inventory.Instance.Items;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < items.Count)
                slots[i].Setup(items[i]);
            else
                slots[i].Setup(null);
        }
        Debug.Log($"[InventoryUI] RefreshSlots 호출 — 아이템 수: {Inventory.Instance?.Items.Count}");
        if (slots == null || Inventory.Instance == null) return;
    }
}