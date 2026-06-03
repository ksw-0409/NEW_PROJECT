using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 역할: 장비창의 슬롯 하나 — 아이템 없으면 아이콘 숨김, 장착 시 아이콘 표시

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("슬롯 타입 (Inspector에서 지정)")]
    public EquipmentSlot slotType;

    [Header("UI 연결")]
    [SerializeField] private Image iconImage;

    private InventoryItem equippedItem;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        //Refresh(null);
    }

    // ─── 갱신 ────────────────────────────────────────
    public void Refresh(InventoryItem item)
    {
        equippedItem = item;

        if (iconImage == null) return;

        if (item == null)
        {
            iconImage.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            iconImage.sprite = item.iconSprite;
            iconImage.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    /* ─── 입력 ─────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedItem == null || InventoryTooltip.Instance == null) return;
        InventoryTooltip.Instance.ShowAt(BuildTooltipText(), rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryTooltip.Instance?.Hide();
    }*/

    public void OnPointerClick(PointerEventData eventData)
    {
        if (equippedItem == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        CharacterEquipmentUI.Instance?.UnequipSlot(slotType);
    }

    // ─── 유틸 ─────────────────────────────────────────
    private string BuildTooltipText()
    {
        var item = equippedItem;
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"{item.itemName}");
        sb.AppendLine($"[{GetSlotKoreanName(slotType)}]");
        sb.AppendLine("[ 옵션 ]");
        if (item.physicalDamage > 0) sb.AppendLine($"물리 공격력: +{item.physicalDamage:F1}");
        if (item.magicDamage > 0) sb.AppendLine($"마법 공격력: +{item.magicDamage:F1}");
        if (item.criticalChance > 0) sb.AppendLine($"치명타 확률: +{item.criticalChance * 100f:F1}%");
        if (item.criticalDamage > 0) sb.AppendLine($"치명타 피해: +{item.criticalDamage:F2}배");
        if (item.maxHealth > 0) sb.AppendLine($"최대 체력:   +{item.maxHealth:F0}");
        if (item.physicalDefense > 0) sb.AppendLine($"방어력:      +{item.physicalDefense:F1}");
        if (item.moveSpeed > 0) sb.AppendLine($"이동속도:    +{item.moveSpeed:F2}");
        if (item.attackcooldown > 0) sb.AppendLine($"쿨타임 감소: -{item.attackcooldown:F2}초");
        sb.Append("\n<color=#888>클릭: 장비 해제</color>");
        return sb.ToString();
    }

    public static string GetSlotKoreanName(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon: return "무기";
            case EquipmentSlot.Helmet: return "투구";
            case EquipmentSlot.Armor: return "상의";
            case EquipmentSlot.Pants: return "하의";
            case EquipmentSlot.Shoes: return "신발";
            default: return slot.ToString();
        }
    }
}