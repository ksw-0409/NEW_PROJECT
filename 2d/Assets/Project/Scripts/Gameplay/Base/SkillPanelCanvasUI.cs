using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 역할: 스킬 패널 Canvas UI

[System.Serializable]
public class SkillEntry
{
    public Button button;           // 스킬 패널의 버튼
    public GameObject skillTreeCanvas; // 해당 스킬의 트리 Canvas
}

public class SkillPanelCanvasUI : BaseCanvasUI
{
    [Header("스킬 목록 (버튼 - 스킬트리)")]
    [SerializeField] private List<SkillEntry> skillEntries = new List<SkillEntry>();

    // 현재 열려있는 스킬 트리 Canvas
    private GameObject currentSkillTree = null;

    protected override void OnOpen()
    {
        // 모든 스킬 버튼에 리스너 등록
        foreach (var entry in skillEntries)
        {
            if (entry.button == null) continue;
            var captured = entry;
            entry.button.onClick.AddListener(() => OnClickSkill(captured.skillTreeCanvas));
        }

        CloseCurrentSkillTree();
    }

    protected override void OnClose()
    {
        // 모든 리스너 해제
        foreach (var entry in skillEntries)
        {
            if (entry.button != null)
                entry.button.onClick.RemoveAllListeners();
        }

        CloseCurrentSkillTree();
    }

    protected override bool CanClose()
    {
        if (currentSkillTree != null && currentSkillTree.activeSelf)
        {
            CloseCurrentSkillTree();
            return false;
        }
        return true;
    }

    private void OnClickSkill(GameObject skillTreeCanvas)
    {
        if (skillTreeCanvas == null)
        {
            Debug.LogWarning("[SkillPanelCanvasUI] skillTreeCanvas가 연결되지 않았습니다.");
            return;
        }

        if (currentSkillTree != null && currentSkillTree != skillTreeCanvas)
            CloseCurrentSkillTree();

        currentSkillTree = skillTreeCanvas;
        currentSkillTree.SetActive(true);
    }

    private void CloseCurrentSkillTree()
    {
        if (currentSkillTree != null)
        {
            currentSkillTree.SetActive(false);
            currentSkillTree = null;
        }
    }
}