using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// 역할: 아이템 강화 UI (아이템 선택 + 강화 버튼 + 골드/비용 표시)
// 강화 로직은 ItemEnhanceManager에 위임하고, UI 표시만 담당
// [임시] 인벤토리 미구현 상태 — 인스펙터에서 테스트 아이템 직접 연결

public class ItemEnhanceUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private ItemEnhanceManager enhanceManager;

    [Header("UI 요소")]
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI currentGoldText;

    [Header("선택된 아이템 표시")]
    [SerializeField] private TextMeshProUGUI selectedItemName;
    [SerializeField] private TextMeshProUGUI selectedItemStats;

    [Header("슬롯 버튼들 (인벤토리)")]
    [SerializeField] private ItemSlotButton[] inventorySlots;

    [Header("임시 테스트 아이템 (인벤토리 구현 전)")]
    [SerializeField] private EquipmentData[] testItems;

    private EquipmentData selectedItem;
    private int selectedSlotIndex = -1;

    // SO 원본값 백업 (게임 시작 시 1회 저장)
    private RuntimeItemData[] originalItems;

    void Start()
    {
        enhanceButton.onClick.AddListener(OnClickEnhance);
        enhanceButton.interactable = false;

        // 게임 시작 시 SO 원본값 백업
        if (testItems != null)
        {
            originalItems = new RuntimeItemData[testItems.Length];
            for (int i = 0; i < testItems.Length; i++)
            {
                if (testItems[i] != null)
                    originalItems[i] = RuntimeItemData.FromEquipmentData(testItems[i]);
            }
        }

        // 게임 시작 시 SO를 원본값으로 복원
        RestoreOriginalItems();
    }

    void OnEnable()
    {
        enhanceManager.OnEnhanceSuccess += HandleSuccess;
        enhanceManager.OnEnhanceFailed += HandleFailed;
        GameDataManager.OnGoldChanged += RefreshGoldUI;

        if (GameDataManager.Instance != null)
            RefreshGoldUI(GameDataManager.Instance.Gold);

        if (costText != null && enhanceManager != null)
            costText.text = $"강화 비용: {enhanceManager.GetEnhanceCost()}G";

        ClearSelection();
        SetupInventorySlots();

        BaseInteractable.IsUIOpen = true;
    }

    void OnDisable()
    {
        enhanceManager.OnEnhanceSuccess -= HandleSuccess;
        enhanceManager.OnEnhanceFailed -= HandleFailed;
        GameDataManager.OnGoldChanged -= RefreshGoldUI;

        if (inventorySlots != null)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot != null)
                    slot.OnSlotClicked -= OnItemSelected;
            }
        }

        BaseInteractable.IsUIOpen = false;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            gameObject.SetActive(false);
    }

    // 게임 시작 시 SO를 원본값으로 복원
    // GameDataManager에 저장된 리롤 데이터가 없으면 백업값으로 되돌림
    private void RestoreOriginalItems()
    {
        if (testItems == null || originalItems == null) return;

        for (int i = 0; i < testItems.Length; i++)
        {
            if (testItems[i] == null || originalItems[i] == null) continue;

            // 저장된 리롤 데이터가 없으면 원본으로 복원
            if (GameDataManager.Instance != null &&
                !GameDataManager.Instance.HasRuntimeItem(i))
            {
                originalItems[i].ApplyTo(testItems[i]);
            }
        }
    }

    private void SetupInventorySlots()
    {
        if (inventorySlots == null) return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null) continue;

            inventorySlots[i].OnSlotClicked += OnItemSelected;
            inventorySlots[i].SetSlotIndex(i);

            if (testItems != null && i < testItems.Length)
            {
                // 씬 전환 후 저장된 리롤 데이터가 있으면 복원
                if (GameDataManager.Instance != null)
                    GameDataManager.Instance.LoadRuntimeItem(i, testItems[i]);

                inventorySlots[i].Setup(testItems[i]);
            }
            else
            {
                inventorySlots[i].Setup(null);
            }
        }
    }

    public void OnItemSelected(EquipmentData item)
    {
        selectedItem = item;

        selectedSlotIndex = -1;
        if (inventorySlots != null)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] != null && inventorySlots[i].GetSlotData() == item)
                {
                    selectedSlotIndex = i;
                    break;
                }
            }
        }

        RefreshSelectedItemUI();
        enhanceButton.interactable = (selectedItem != null);
    }

    private void OnClickEnhance()
    {
        enhanceManager.TryEnhance(selectedItem, selectedSlotIndex);
    }

    private void HandleSuccess(EquipmentData enhanced)
    {
        RefreshSelectedItemUI();
    }

    private void HandleFailed(string reason)
    {
        Debug.Log($"[ItemEnhanceUI] 강화 실패: {reason}");
    }

    private void RefreshGoldUI(int gold)
    {
        if (currentGoldText != null)
            currentGoldText.text = $"보유 골드: {gold}G";
    }

    private void RefreshSelectedItemUI()
    {
        if (selectedItem == null)
        {
            ClearSelection();
            return;
        }

        if (selectedItemName != null)
            selectedItemName.text = selectedItem.itemName;

        if (selectedItemStats != null)
            selectedItemStats.text = BuildStatText(selectedItem);
    }

    private void ClearSelection()
    {
        selectedItem = null;
        selectedSlotIndex = -1;
        if (selectedItemName != null) selectedItemName.text = "";
        if (selectedItemStats != null) selectedItemStats.text = "아이템을 선택하세요";
        enhanceButton.interactable = false;
    }

    private string BuildStatText(EquipmentData item)
    {
        var sb = new System.Text.StringBuilder();
        if (item.physicalDamage > 0) sb.AppendLine($"물리 공격력: {item.physicalDamage:F1}");
        if (item.magicDamage > 0) sb.AppendLine($"마법 공격력: {item.magicDamage:F1}");
        if (item.criticalChance > 0) sb.AppendLine($"치명타 확률: {item.criticalChance * 100f:F1}%");
        if (item.criticalDamage > 0) sb.AppendLine($"치명타 피해: {item.criticalDamage:F2}배");
        if (item.maxHealth > 0) sb.AppendLine($"최대 체력: {item.maxHealth:F1}");
        if (item.physicalDefense > 0) sb.AppendLine($"방어력: {item.physicalDefense:F1}");
        if (item.moveSpeed > 0) sb.AppendLine($"이동속도: {item.moveSpeed:F2}");
        return sb.ToString();
    }
}