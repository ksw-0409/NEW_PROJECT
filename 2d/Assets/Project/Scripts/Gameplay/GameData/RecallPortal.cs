using UnityEngine;

// 역할: 거점으로 돌아가는 포탈

public class RecallPortal : BaseInteractable
{
    protected override void HandleInteract()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[RecallPortal] SceneController가 없습니다.");
            return;
        }

        PlayerStats.Instance?.HealToFull();
        GameDataManager.Instance.ClearSavedSkills();
        SceneController.Instance.LoadBase();
    }

}