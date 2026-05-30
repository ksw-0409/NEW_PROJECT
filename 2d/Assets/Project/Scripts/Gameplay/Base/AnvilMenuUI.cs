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

    [SerializeField] private GameObject menuPanel; // 버튼들이 있는 패널

    protected override void OnOpen()
    {
        identifyButton.onClick.AddListener(OnClickIdentify);
        enhanceButton.onClick.AddListener(OnClickEnhance);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    protected override void OnClose()
    {
        identifyButton.onClick.RemoveAllListeners();
        enhanceButton.onClick.RemoveAllListeners();
    }

    private void OnClickIdentify()
    {
        if (identifyCanvas == null) return;
        if (menuPanel != null) menuPanel.SetActive(false);
        identifyCanvas.SetActive(true);
    }

    private void OnClickEnhance()
    {
        if (enhanceCanvas == null) return;
        if (menuPanel != null) menuPanel.SetActive(false);
        enhanceCanvas.SetActive(true);
    }
}