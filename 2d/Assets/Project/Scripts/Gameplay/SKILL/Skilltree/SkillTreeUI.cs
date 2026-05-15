using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeUI : MonoBehaviour
{
    public static SkillTreeUI Instance;

    public SkillNode selectedNode;

    [Header("해금 버튼")]
    public Button unlockButton;

    [Header("정보 패널")]
    public GameObject      infoPanel;
    public TextMeshProUGUI infoNodeNameText;
    public TextMeshProUGUI infoNodeDescText;
    public TextMeshProUGUI infoNodeCostText;
    public TextMeshProUGUI infoNodeStatusText;

    [Tooltip("선택된 노드의 아이콘을 표시할 Image (선택사항 — InfoPanel 안의 Icon)")]
    public Image           infoNodeIcon;

    void Awake() => Instance = this;

    void Start()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
        SetButtonInteractable(false);
    }

    // ─── 노드 선택 ─────────────────────────
    public void OnNodeSelected(SkillNode node)
    {
        if (selectedNode != null) selectedNode.SetSelected(false);
        selectedNode = node;
        if (selectedNode != null) selectedNode.SetSelected(true);
        UpdateInfoPanel();
        UpdateButtonState();
    }

    // ─── 정보 패널 ─────────────────────────
    private void UpdateInfoPanel()
    {
        if (infoPanel == null) return;
        if (selectedNode == null) { infoPanel.SetActive(false); return; }

        infoPanel.SetActive(true);

        if (infoNodeNameText != null)
            infoNodeNameText.text = string.IsNullOrEmpty(selectedNode.nodeName)
                ? selectedNode.name : selectedNode.nodeName;

        if (infoNodeDescText != null)
            infoNodeDescText.text = selectedNode.nodeDescription;

        if (infoNodeCostText != null)
            infoNodeCostText.text = $"<color=#FFD700>◆ 비용: {selectedNode.unlockCost}</color>";

        if (infoNodeStatusText != null)
        {
            if (selectedNode.IsUnlocked)
                infoNodeStatusText.text = "<color=#FFD700><b>✔ 해금됨</b></color>";
            else if (selectedNode.IsBlockedByExclusive())
                infoNodeStatusText.text = "<color=#666666>✕ 다른 분기 선택됨</color>";
            else if (!selectedNode.CanUnlock())
                infoNodeStatusText.text = "<color=#888888>✕ 선행 조건 필요</color>";
            else
                infoNodeStatusText.text = "<color=#7BCFFF>☆ 해금 가능</color>";
        }

        if (infoNodeIcon != null)
        {
            UnityEngine.Sprite preview = selectedNode.iconUnlocked != null
                ? selectedNode.iconUnlocked
                : (selectedNode.iconImage != null ? selectedNode.iconImage.sprite : null);
            infoNodeIcon.sprite = preview;
            infoNodeIcon.enabled = preview != null;
            infoNodeIcon.color = UnityEngine.Color.white;
        }
    }

    // ─── 버튼 상태 ─────────────────────────
    public void UpdateButtonState()
    {
        if (selectedNode == null)       { SetButtonInteractable(false); return; }
        if (selectedNode.IsUnlocked)    { SetButtonInteractable(false); return; }
        if (!selectedNode.CanUnlock())  { SetButtonInteractable(false); return; }

        // 테스트 모드: 재화 체크 없이 항상 활성화
#if UNITY_EDITOR
        SetButtonInteractable(true);
        return;
#endif
        int cur = GameDataManager.Instance != null ? GameDataManager.Instance.NormalCurrency : 0;
        SetButtonInteractable(cur >= selectedNode.unlockCost);
    }

    private void SetButtonInteractable(bool v)
    {
        if (unlockButton == null) return;
        unlockButton.interactable = v;

        // 버튼 텍스트 색도 같이 변경
        var txt = unlockButton.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
            txt.color = v ? UnityEngine.Color.white : new UnityEngine.Color(0.5f,0.5f,0.5f,0.6f);
    }

    // ─── 해금 클릭 ─────────────────────────
    public void OnClickUnlockButton()
    {
        if (selectedNode == null || !selectedNode.CanUnlock()) return;

        selectedNode.TryUnlock();

        // 해금 후 패널 갱신만, 선택 유지
        UpdateInfoPanel();
        UpdateButtonState();
    }
}
