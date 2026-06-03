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

        ApplyGradeBorder(item.Grade);
        button.interactable = true;
    }

    private void ClearSlot()
    {
        if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
        if (itemNameText != null) itemNameText.text = "";
        ApplyGradeBorder(null);
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

    private void ApplyGradeBorder(ItemGrade? grade)
    {
        Transform borderT = transform.Find("GradeBorder");
        Image borderImg;
        if (borderT == null)
        {
            var go = new GameObject("GradeBorder");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            borderImg = go.AddComponent<Image>();
            borderImg.raycastTarget = false;
            borderImg.sprite = null;
            go.transform.SetAsFirstSibling();
        }
        else borderImg = borderT.GetComponent<Image>();
        if (borderImg == null) return;
        if (grade.HasValue)
        {
            Color c = GetGradeColor(grade.Value);
            c.a = 0.45f;
            borderImg.color = c;
        }
        else borderImg.color = new Color(0f, 0f, 0f, 0f);
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}