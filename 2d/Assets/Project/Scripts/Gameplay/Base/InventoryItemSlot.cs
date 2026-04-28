using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

// 역할: 감정/강화 UI에서 사용하는 인벤토리 슬롯
// RectTransform 범위 체크로 툴팁 표시

public class InventoryItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotData == null || InventoryTooltip.Instance == null) return;
        InventoryTooltip.Instance.Show(BuildTooltipText(slotData));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.Hide();
    }

    private string BuildTooltipText(InventoryItem item)
    {
        var sb = new System.Text.StringBuilder();
        string hexColor = ColorUtility.ToHtmlStringRGB(GetGradeColor(item.Grade));
        sb.AppendLine($"<color=#{hexColor}>{item.itemName}</color>");

        if (!item.isIdentified)
        {
            sb.AppendLine("미감정 아이템");
        }
        else
        {
            sb.AppendLine("[ 옵션 ]");
            if (item.physicalDamage > 0) sb.AppendLine($"물리 공격력: {item.physicalDamage:F1}");
            if (item.magicDamage > 0) sb.AppendLine($"마법 공격력: {item.magicDamage:F1}");
            if (item.criticalChance > 0) sb.AppendLine($"치명타 확률: {item.criticalChance * 100f:F1}%");
            if (item.criticalDamage > 0) sb.AppendLine($"치명타 피해: {item.criticalDamage:F2}배");
            if (item.maxHealth > 0) sb.AppendLine($"최대 체력: {item.maxHealth:F1}");
            if (item.physicalDefense > 0) sb.AppendLine($"방어력: {item.physicalDefense:F1}");
            if (item.moveSpeed > 0) sb.AppendLine($"이동속도: {item.moveSpeed:F2}");
        }
        return sb.ToString();
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
            case ItemGrade.Rare: return new Color(0.2f, 0.5f, 1f);
            case ItemGrade.Epic: return new Color(0.6f, 0.2f, 1f);
            case ItemGrade.Legendary: return new Color(1f, 0.85f, 0f);
            default: return Color.white;
        }
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}