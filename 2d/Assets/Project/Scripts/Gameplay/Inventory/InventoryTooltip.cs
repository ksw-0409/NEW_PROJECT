using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// 역할: Canvas에 하나만 존재하는 공유 툴팁
// CanvasGroup 알파로 표시/숨김 처리 (SetActive 미사용 → 깜빡임 방지)

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tooltipText;

    private RectTransform tooltipRect;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;
        tooltipRect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        tooltipRect.pivot = new Vector2(1f, 1f); // 오른쪽 상단 기준

        // 시작 시 숨김
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show(string text)
    {
        if (tooltipText != null)
            tooltipText.text = text;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
        }

        UpdatePosition(Mouse.current.position.ReadValue());
    }
    public void ShowAt(string text, RectTransform slotRect)
    {

        if (tooltipText != null)
            tooltipText.text = text;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
        }

        // 슬롯 오른쪽 상단 기준으로 위치 설정
        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(null, corners[2]),
            rootCanvas.worldCamera,
            out Vector2 localPos
        );

        tooltipRect.anchoredPosition = localPos;
    }

    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void UpdatePosition(Vector2 screenPos)
    {
        if (rootCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.worldCamera,
            out Vector2 localPos
        );

        tooltipRect.anchoredPosition = localPos;
    }
}