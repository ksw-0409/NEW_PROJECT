using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 인벤토리 UI (Tab 키 열고 닫기, 슬롯 표시)
// ✨ 장비창(CharacterEquipmentUI)과 함께 열리고 닫힘
// 전투 중(스테이지 진행 중)에는 열 수 없음

public class InventoryUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventorySlot[] slots; // 4x7 = 28슬롯

    [Header("✨ 장비창 패널 (같이 열고 닫힘)")]
    [SerializeField] private GameObject equipmentPanel;

    void OnEnable()
    {
        Inventory.OnInventoryChanged += RefreshSlots;
    }

    void OnDisable()
    {
        Inventory.OnInventoryChanged -= RefreshSlots;

        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
            if (equipmentPanel != null) equipmentPanel.SetActive(false);
            BaseInteractable.IsUIOpen = false;
        }
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // ⭐ 창고 열린 상태에서 Tab → 창고 + 인벤토리 모두 닫기
            if (StashUI.Instance != null && StashUI.Instance.IsOpen)
            {
                StashUI.Instance.Close();
                return;
            }

            if (inventoryPanel.activeSelf)
            {
                CloseAll();
                return;
            }

            if (BaseInteractable.IsUIOpen) return;

            // 전투 중(스테이지 진행 중)이면 열 수 없음
            if (StageManager.IsStageActive && !StageManager.IsStageOver)
            {
                Debug.Log("[InventoryUI] 전투 중에는 인벤토리를 열 수 없습니다.");
                return;
            }

            OpenAll();
        }
    }

    private void OpenAll()
    {
        inventoryPanel.SetActive(true);
        if (equipmentPanel != null) equipmentPanel.SetActive(true);
        BaseInteractable.IsUIOpen = true;
        RefreshSlots();
    }

    private void CloseAll()
    {
        inventoryPanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        BaseInteractable.IsUIOpen = false;
    }

    // ✨ CharacterEquipmentUI에서 장착/해제 후 슬롯 갱신 요청 시 사용
    public void ForceRefresh() => RefreshSlots();

    private void RefreshSlots()
    {
        if (slots == null || Inventory.Instance == null) return;

        var items = Inventory.Instance.Items;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            slots[i].Setup(i < items.Count ? items[i] : null);
        }

        Debug.Log($"[InventoryUI] RefreshSlots — 아이템 수: {Inventory.Instance?.Items.Count}");
    }
}