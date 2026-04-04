using UnityEngine;

// 역할: 모닥불 오브젝트 — F키로 SkillPanel Canvas 활성화

public class CampfireInteraction : BaseInteractable
{
    [SerializeField] private GameObject skillPanelCanvas;

    protected override void HandleInteract()
    {
        if (skillPanelCanvas == null)
        {
            Debug.LogWarning("[CampfireInteraction] skillPanelCanvas가 연결되지 않았습니다.");
            return;
        }

        skillPanelCanvas.SetActive(true);
    }
}