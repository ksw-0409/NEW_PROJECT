using UnityEngine;

// 역할: 모루 오브젝트 — F키로 Anvil Canvas 활성화 + IsUIOpen 처리

public class AnvilInteraction : BaseInteractable
{
    [SerializeField] private GameObject anvilCanvas;

    protected override void HandleInteract()
    {
        if (anvilCanvas == null)
        {
            Debug.LogWarning("[AnvilInteraction] anvilCanvas가 연결되지 않았습니다.");
            return;
        }

        anvilCanvas.SetActive(true);
        // Canvas의 ItemEnhanceUI.OnEnable()에서 IsUIOpen = true 처리함
    }
}