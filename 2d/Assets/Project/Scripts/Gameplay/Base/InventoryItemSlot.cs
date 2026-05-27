using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

// ����: ����/��ȭ UI���� ����ϴ� �κ��丮 ����

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
            iconImage.sprite = item.iconSprite;
            iconImage.color = item.iconSprite != null
                ? Color.white
                : new Color(1f, 1f, 1f, 0f);
            iconImage.enabled = true;
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
        if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
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
            case ItemGrade.Common:    return new Color(0.90f,0.90f,0.90f);
            case ItemGrade.Rare:      return new Color(0.30f,0.55f,1.00f);
            case ItemGrade.Epic:      return new Color(0.75f,0.30f,1.00f);
            case ItemGrade.Unique:    return new Color(1.00f,0.35f,0.70f);
            case ItemGrade.Legendary: return new Color(1.00f,0.75f,0.20f);
            default: return Color.white;
        }
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}