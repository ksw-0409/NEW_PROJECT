using UnityEngine;

/// <summary>
/// 창고 상호작용 오브젝트 — 플레이어가 범위에 들어가서 F 키 누르면 창고 UI 열림.
/// 감정/스킬트리 NPC와 동일한 패턴 (BaseInteractable).
/// </summary>
public class StashInteractable : BaseInteractable
{
    protected override void HandleInteract()
    {
        if (StashUI.Instance == null)
        {
            Debug.LogWarning("[StashInteractable] StashUI.Instance가 없습니다.");
            return;
        }
        StashUI.Instance.Open();
    }
}
