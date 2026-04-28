using UnityEngine;
using UnityEngine.UI;

// 역할: 모루 상호작용 시 나타나는 메뉴 UI
// 감정 버튼 / 옵션 변경 버튼 선택

public class AnvilMenuUI : BaseCanvasUI
{
    [Header("버튼")]
    [SerializeField] private Button identifyButton;
    [SerializeField] private Button enhanceButton;

    [Header("연결할 Canvas")]
    [SerializeField] private GameObject identifyCanvas;
    [SerializeField] private GameObject enhanceCanvas;

    protected override void OnOpen()
    {
        identifyButton.onClick.AddListener(OnClickIdentify);
        enhanceButton.onClick.AddListener(OnClickEnhance);
    }

    protected override void OnClose()
    {
        identifyButton.onClick.RemoveAllListeners();
        enhanceButton.onClick.RemoveAllListeners();
    }

    private void OnClickIdentify()
    {
        if (identifyCanvas == null) return;
        gameObject.SetActive(false);
        identifyCanvas.SetActive(true);
    }

    private void OnClickEnhance()
    {
        if (enhanceCanvas == null) return;
        gameObject.SetActive(false);
        enhanceCanvas.SetActive(true);
    }
}