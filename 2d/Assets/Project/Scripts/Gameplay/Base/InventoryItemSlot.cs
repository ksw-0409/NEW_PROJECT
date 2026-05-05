using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

// 역할: 감정/강화 UI에서 사용하는 인벤토리 슬롯

public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Button button;

    public event Action<InventoryItem> OnSlotClicked;

    private InventoryItem slotData;

    void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    public void Setup(InventoryItem item)
    {
        slotData = item;

        if (item == null)
        {
            ClearSlot();
            return;
        }

        if (iconImage != null)
        {
            Sprite icon = Resources.Load<Sprite>("Icons/" + item.iconName);
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (itemNameText != null)
        {
            itemNameText.text = item.isIdentified ? item.itemName : "???";
            itemNameText.color = GetGradeColor(item.Grade);
        }

        button.interactable = true;
    }

    private void ClearSlot()
    {
        if (iconImage != null) iconImage.enabled = false;
        if (itemNameText != null) itemNameText.text = "";
        button.interactable = false;
    }

    private void OnClick()
    {
        if (slotData == null) return;
        OnSlotClicked?.Invoke(slotData);
    }

    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Rare: return Color.blue;
            case ItemGrade.Epic: return Color.yellow;
            case ItemGrade.Legendary: return Color.green;
            default: return Color.white;
        }
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}