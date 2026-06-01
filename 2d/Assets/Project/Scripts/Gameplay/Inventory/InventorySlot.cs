using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 역할: Tab 인벤토리 슬롯
// 호버 툴팁 + 좌클릭 장착

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    private InventoryItem slotItem;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(InventoryItem item)
    {
        slotItem = item;

        if (item == null)
        {
            ClearSlot();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = item.iconSprite;
            iconImage.color = item.iconSprite != null
                ? new Color(1f, 1f, 1f, 1f)
                : new Color(1f, 1f, 1f, 0f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotItem == null || InventoryTooltip.Instance == null) return;
        InventoryTooltip.Instance.ShowAt(BuildTooltipText(slotItem), rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryTooltip.Instance?.Hide();
    }

    // ✨ 좌클릭 → 장비 장착 시도
    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotItem == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (CharacterEquipmentUI.Instance == null)
        {
            Debug.LogWarning("[InventorySlot] CharacterEquipmentUI.Instance가 없습니다.");
            return;
        }

        CharacterEquipmentUI.Instance.TryEquip(slotItem);
    }

    private string BuildTooltipText(InventoryItem item)
    {
        var sb = new System.Text.StringBuilder();
        string hexColor = ColorUtility.ToHtmlStringRGB(GetGradeColor(item.Grade));
        sb.AppendLine($"<color=#{hexColor}>{item.itemName}</color>");

        // 기본 능력치는 항상 표시
        sb.AppendLine("[ 기본 능력치 ]");
        if (item.basePhysicalDamage > 0) sb.AppendLine($"물리 공격력: {item.basePhysicalDamage:F1}");
        if (item.baseMagicDamage > 0) sb.AppendLine($"마법 공격력: {item.baseMagicDamage:F1}");
        if (item.baseCriticalChance > 0) sb.AppendLine($"치명타 확률: {item.baseCriticalChance:F1}%");
        if (item.baseCriticalDamage > 0) sb.AppendLine($"치명타 피해: {item.baseCriticalDamage:F2}배");
        if (item.baseMaxHealth > 0) sb.AppendLine($"최대 체력:   {item.baseMaxHealth:F0}");
        if (item.basePhysicalDefense > 0) sb.AppendLine($"방어력:      {item.basePhysicalDefense:F1}");
        if (item.baseMoveSpeed > 0) sb.AppendLine($"이동속도:    {item.baseMoveSpeed:F2}");
        if (item.baseAttackcooldown > 0) sb.AppendLine($"쿨타임 감소: -{item.baseAttackcooldown:F2}초");

        if (!item.isIdentified)
        {
            sb.AppendLine("\n[ 옵션 ]");
            sb.AppendLine("???");
            sb.Append("\n<color=#888>감정 후 장착 가능</color>");
        }
        else
        {
            float addPhys = item.physicalDamage - item.basePhysicalDamage;
            float addMagic = item.magicDamage - item.baseMagicDamage;
            float addCrit = item.criticalChance - item.baseCriticalChance;
            float addCritDmg = item.criticalDamage - item.baseCriticalDamage;
            float addHp = item.maxHealth - item.baseMaxHealth;
            float addDef = item.physicalDefense - item.basePhysicalDefense;
            float addSpeed = item.moveSpeed - item.baseMoveSpeed;

            sb.AppendLine("\n[ 옵션 ]");
            if (addPhys != 0) sb.AppendLine($"물리 공격력: {addPhys:+0.0;-0.0}");
            if (addMagic != 0) sb.AppendLine($"마법 공격력: {addMagic:+0.0;-0.0}");
            if (addCrit != 0) sb.AppendLine($"치명타 확률: {addCrit:+0.0;-0.0}%");
            if (addCritDmg != 0) sb.AppendLine($"치명타 피해: {addCritDmg:+0.00;-0.00}배");
            if (addHp != 0) sb.AppendLine($"최대 체력:   {addHp:+0;-0}");
            if (addDef != 0) sb.AppendLine($"방어력:      {addDef:+0.0;-0.0}");
            if (addSpeed != 0) sb.AppendLine($"이동속도:    {addSpeed:+0.00;-0.00}");
            if (item.attackcooldown > 0) sb.AppendLine($"쿨타임 감소: -{item.attackcooldown:F2}초");

            sb.Append("\n<color=#aaa>클릭: 장착</color>");
        }

        return sb.ToString();
    }

    private void ClearSlot()
    {
        if (iconImage != null)
            iconImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return new Color(0.90f, 0.90f, 0.90f);
            case ItemGrade.Rare: return new Color(0.30f, 0.55f, 1.00f);
            case ItemGrade.Epic: return new Color(0.75f, 0.30f, 1.00f);
            case ItemGrade.Unique: return new Color(1.00f, 0.35f, 0.70f);
            case ItemGrade.Legendary: return new Color(1.00f, 0.75f, 0.20f);
            default: return Color.white;
        }
    }
}