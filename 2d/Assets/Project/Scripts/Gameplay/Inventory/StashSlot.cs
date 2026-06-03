using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 창고(Stash) 슬롯 — InventorySlot과 시각 동일하지만 클릭 시 창고 → 인벤토리로 이동.
/// 호버 툴팁 + 등급 테두리 적용.
/// </summary>
public class StashSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image gradeBorderImage;

    private InventoryItem slotItem;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // iconImage 자동 탐색 (자식 Image 컴포넌트)
        if (iconImage == null)
        {
            var childImg = transform.Find("Image");
            if (childImg != null) iconImage = childImg.GetComponent<Image>();
        }
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

        ApplyGradeBorder(item.Grade);
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
        else
        {
            borderImg = borderT.GetComponent<Image>();
        }

        if (borderImg == null) return;
        if (grade.HasValue)
        {
            Color c = InventorySlot.GetGradeColor(grade.Value);
            c.a = 0.45f;
            borderImg.color = c;
        }
        else
        {
            borderImg.color = new Color(0f, 0f, 0f, 0f);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotItem == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (Stash.Instance == null) return;
        Stash.Instance.TransferToInventory(slotItem);
    }

    private void ClearSlot()
    {
        if (iconImage != null)
            iconImage.color = new Color(1f, 1f, 1f, 0f);
        ApplyGradeBorder(null);
    }

    private string BuildTooltipText(InventoryItem item)
    {
        var sb = new System.Text.StringBuilder();
        string hexColor = ColorUtility.ToHtmlStringRGB(InventorySlot.GetGradeColor(item.Grade));
        sb.AppendLine($"<color=#{hexColor}>{item.itemName}</color>");
        sb.AppendLine("\n[ 창고 보관 중 ]");
        sb.AppendLine("<color=#aaa>클릭: 인벤토리로 이동</color>");
        return sb.ToString();
    }
}
