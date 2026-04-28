using UnityEngine;

// 역할: 모루 오브젝트 — F키로 AnvilMenu Canvas 활성화

public class AnvilInteraction : BaseInteractable
{
    [SerializeField] private GameObject anvilMenuCanvas; // 메뉴 선택 Canvas

    protected override void HandleInteract()
    {
        if (anvilMenuCanvas == null)
        {
            Debug.LogWarning("[AnvilInteraction] anvilMenuCanvas가 연결되지 않았습니다.");
            return;
        }
        anvilMenuCanvas.SetActive(true);
    }
}