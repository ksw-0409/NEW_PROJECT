using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 역할: 아이템 강화 UI (아이템 선택 + 강화 버튼 + 골드/비용 표시)
// BaseCanvasUI 상속으로 ESC, IsUIOpen, PlayerInput 처리 자동화

public class ItemEnhanceUI : BaseCanvasUI
{
    [Header("연결")]
    [SerializeField] private ItemEnhanceManager enhanceManager;

    [Header("UI 요소")]
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI currentGoldText;

    [Header("선택된 아이템 표시")]
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private TextMeshProUGUI selectedItemName;

    [Header("옵션 표시 TMP 배열 (옵션 하나당 TMP 하나)")]
    [SerializeField] private TextMeshProUGUI[] optionTexts;

    [Header("인벤토리 슬롯")]
    [SerializeField] private InventoryItemSlot[] inventorySlots;

    private InventoryItem selectedItem;

    protected override void OnOpen()
    {
        enhanceButton.onClick.AddListener(OnClickEnhance);
        enhanceButton.interactable = false;

        enhanceManager.OnEnhanceSuccess += HandleSuccess;
        enhanceManager.OnEnhanceFailed += HandleFailed;
        GameDataManager.OnGoldChanged += RefreshGoldUI;

        if (GameDataManager.Instance != null)
            RefreshGoldUI(GameDataManager.Instance.Gold);

        if (costText != null)
            costText.text = $"{enhanceManager.GetEnhanceCost()}G";

        ClearSelection();
        ClearOptionTexts();
        SetupSlots();
    }

    protected override void OnClose()
    {
        enhanceButton.onClick.RemoveAllListeners();

        enhanceManager.OnEnhanceSuccess -= HandleSuccess;
        enhanceManager.OnEnhanceFailed -= HandleFailed;
        GameDataManager.OnGoldChanged -= RefreshGoldUI;

        if (inventorySlots != null)
        {
            foreach (var slot in inventorySlots)
                if (slot != null) slot.OnSlotClicked -= OnItemSelected;
        }
    }

    private void SetupSlots()
    {
        if (inventorySlots == null || Inventory.Instance == null) return;

        var items = Inventory.Instance.Items;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null) continue;

            inventorySlots[i].OnSlotClicked += OnItemSelected;

            if (i < items.Count)
                inventorySlots[i].Setup(items[i]);
            else
                inventorySlots[i].Setup(null);
        }
    }

    public void OnItemSelected(InventoryItem item)
    {
        selectedItem = item;

        if (selectedItemName != null)
            selectedItemName.text = item != null ? item.itemName : "아이템을 선택하세요";

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = item?.iconSprite;
            selectedItemIcon.color = item?.iconSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
        }

        if (item != null && item.isIdentified)
            RefreshOptionTexts(item);
        else
            ClearOptionTexts();

        enhanceButton.interactable = (item != null && item.isIdentified);
    }

    private void OnClickEnhance()
    {
        enhanceManager.TryEnhance(selectedItem);
    }

    private void HandleSuccess(InventoryItem enhanced)
    {
        RefreshOptionTexts(enhanced);
    }

    private void HandleFailed(string reason)
    {
        Debug.Log($"[ItemEnhanceUI] 강화 실패: {reason}");
    }

    private void RefreshGoldUI(int gold)
    {
        if (currentGoldText != null)
            currentGoldText.text = $"{gold}G";
    }

    private void RefreshOptionTexts(InventoryItem item)
    {
        if (optionTexts == null) return;

        string[] options = BuildOptionArray(item);

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] == null) continue;
            optionTexts[i].text = i < options.Length ? options[i] : "";
        }
    }

    private void ClearOptionTexts()
    {
        if (optionTexts == null) return;
        foreach (var t in optionTexts)
            if (t != null) t.text = "";
    }

    private string[] BuildOptionArray(InventoryItem item)
    {
        var list = new System.Collections.Generic.List<string>();
        if (item.physicalDamage > 0) list.Add($"물리 공격력: {item.physicalDamage:F1}");
        if (item.magicDamage > 0) list.Add($"마법 공격력: {item.magicDamage:F1}");
        if (item.criticalChance > 0) list.Add($"치명타 확률: {item.criticalChance * 100f:F1}%");
        if (item.criticalDamage > 0) list.Add($"치명타 피해: {item.criticalDamage:F2}배");
        if (item.maxHealth > 0) list.Add($"최대 체력: {item.maxHealth:F1}");
        if (item.physicalDefense > 0) list.Add($"방어력: {item.physicalDefense:F1}");
        if (item.moveSpeed > 0) list.Add($"이동속도: {item.moveSpeed:F2}");
        return list.ToArray();
    }

    private void ClearSelection()
    {
        selectedItem = null;
        if (selectedItemName != null) selectedItemName.text = "아이템을 선택하세요";
        if (selectedItemIcon != null) selectedItemIcon.color = new Color(1f, 1f, 1f, 0f);
        enhanceButton.interactable = false;
    }
}