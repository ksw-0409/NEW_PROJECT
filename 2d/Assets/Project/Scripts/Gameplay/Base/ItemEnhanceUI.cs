using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 역할: 아이템 강화 UI

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

    [Header("옵션 표시 TMP 배열")]
    [SerializeField] private TextMeshProUGUI[] optionTexts;

    [Header("옵션 잠금 버튼 배열 (optionTexts와 순서 동일)")]
    [SerializeField] private Button[] lockButtons;

    [Header("잠금/풀림 이미지")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

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

        HideAllLockButtons();
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

        if (lockButtons != null)
            foreach (var btn in lockButtons)
                if (btn != null) btn.onClick.RemoveAllListeners();

        if (inventorySlots != null)
            foreach (var slot in inventorySlots)
                if (slot != null) slot.OnSlotClicked -= OnItemSelected;
    }

    private void SetupSlots()
    {
        if (inventorySlots == null || Inventory.Instance == null) return;
        var items = Inventory.Instance.Items;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null) continue;
            inventorySlots[i].OnSlotClicked += OnItemSelected;
            inventorySlots[i].Setup(i < items.Count ? items[i] : null);
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
        {
            item.BuildOptions(); // 동적 옵션 생성
            RefreshOptionTexts();
            RefreshLockButtons();
        }
        else
        {
            ClearOptionTexts();
            HideAllLockButtons();
        }

        enhanceButton.interactable = (item != null && item.isIdentified);
    }

    private void OnClickEnhance()
    {
        enhanceManager.TryEnhance(selectedItem);
    }

    private void OnClickLock(int index)
    {
        if (selectedItem == null || selectedItem.options == null) return;
        if (index >= selectedItem.options.Count) return;

        selectedItem.options[index].isLocked = !selectedItem.options[index].isLocked;
        RefreshLockButtonVisual(index);
    }

    private void RefreshLockButtons()
    {
        if (lockButtons == null || selectedItem?.options == null) return;

        for (int i = 0; i < lockButtons.Length; i++)
        {
            if (lockButtons[i] == null) continue;

            if (i < selectedItem.options.Count)
            {
                lockButtons[i].gameObject.SetActive(true);
                lockButtons[i].onClick.RemoveAllListeners();
                int index = i;
                lockButtons[i].onClick.AddListener(() => OnClickLock(index));
                RefreshLockButtonVisual(i);
            }
            else
            {
                lockButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void RefreshLockButtonVisual(int index)
    {
        if (lockButtons == null || index >= lockButtons.Length) return;
        if (lockButtons[index] == null) return;
        if (selectedItem?.options == null || index >= selectedItem.options.Count) return;

        var img = lockButtons[index].GetComponent<Image>();
        if (img != null)
            img.sprite = selectedItem.options[index].isLocked ? lockedSprite : unlockedSprite;
    }

    private void HideAllLockButtons()
    {
        if (lockButtons == null) return;
        foreach (var btn in lockButtons)
            if (btn != null) btn.gameObject.SetActive(false);
    }

    private void RefreshOptionTexts()
    {
        if (optionTexts == null || selectedItem?.options == null) return;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] == null) continue;
            optionTexts[i].text = i < selectedItem.options.Count
                ? selectedItem.options[i].displayText : "";
        }
    }

    private void ClearOptionTexts()
    {
        if (optionTexts == null) return;
        foreach (var t in optionTexts)
            if (t != null) t.text = "";
    }

    private void HandleSuccess(InventoryItem enhanced)
    {
        enhanced.RefreshOptionTexts(); // 잠금 유지하며 텍스트만 갱신
        RefreshOptionTexts();
        RefreshLockButtons();
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

    private void ClearSelection()
    {
        selectedItem = null;
        if (selectedItemName != null) selectedItemName.text = "아이템을 선택하세요";
        if (selectedItemIcon != null) selectedItemIcon.color = new Color(1f, 1f, 1f, 0f);
        enhanceButton.interactable = false;
    }
}