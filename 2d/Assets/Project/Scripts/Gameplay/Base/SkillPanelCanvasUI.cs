using UnityEngine;
using UnityEngine.UI;

// 역할: 스킬 패널 Canvas UI

public class SkillPanelCanvasUI : BaseCanvasUI
{
    [Header("스킬 버튼")]
    [SerializeField] private Button slashSkillButton;

    [Header("스킬 트리 Canvas")]
    [SerializeField] private GameObject skillTreeCanvas;

    protected override void OnOpen()
    {
        slashSkillButton.onClick.AddListener(OnClickSlashSkill);
    }

    protected override void OnClose()
    {
        slashSkillButton.onClick.RemoveListener(OnClickSlashSkill);

        // 스킬 트리가 열려있으면 같이 닫기
        if (skillTreeCanvas != null && skillTreeCanvas.activeSelf)
            skillTreeCanvas.SetActive(false);
    }

    // 스킬 트리가 열려있으면 ESC는 스킬 트리를 먼저 닫아야 함
    protected override bool CanClose()
    {
        if (skillTreeCanvas != null && skillTreeCanvas.activeSelf)
        {
            skillTreeCanvas.SetActive(false);
            return false;
        }
        return true;
    }

    private void OnClickSlashSkill()
    {
        if (skillTreeCanvas == null)
        {
            Debug.LogWarning("[SkillPanelCanvasUI] skillTreeCanvas가 연결되지 않았습니다.");
            return;
        }

        skillTreeCanvas.SetActive(true);
    }
}