using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 역할: 인벤토리 슬롯 하나를 담당하는 버튼
// 아이템 데이터를 표시하고, 클릭 시 이벤트로 상위에 알림

public class ItemSlotButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private Button button;

    public event Action<EquipmentData> OnSlotClicked;

    private EquipmentData slotData;
    private int slotIndex;

    void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    public void Setup(EquipmentData data)
    {
        slotData = data;

        if (data == null)
        {
            ClearSlot();
            return;
        }

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (itemNameText != null)
        {
            itemNameText.text = data.itemName;
            itemNameText.color = GetGradeColor(data.grade);
        }

        if (gradeText != null)
        {
            gradeText.text = data.grade.ToString();
            gradeText.color = GetGradeColor(data.grade);
        }

        button.interactable = true;
    }

    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Rare: return new Color(0.2f, 0.5f, 1f);    // 파란색
            case ItemGrade.Epic: return new Color(0.6f, 0.2f, 1f);    // 보라색
            case ItemGrade.Legendary: return new Color(1f, 0.85f, 0f);   // 노란색
            default: return Color.white;                   // Common 흰색
        }
    }

    public void SetSlotIndex(int index) => slotIndex = index;
    public EquipmentData GetSlotData() => slotData;

    private void ClearSlot()
    {
        if (iconImage != null) iconImage.sprite = null;
        if (itemNameText != null) itemNameText.text = "";
        if (gradeText != null) gradeText.text = "";
        button.interactable = false;
    }

    private void OnClick()
    {
        if (slotData == null) return;
        OnSlotClicked?.Invoke(slotData);
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}