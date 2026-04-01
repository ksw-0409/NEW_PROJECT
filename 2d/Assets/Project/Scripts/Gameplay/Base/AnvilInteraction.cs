using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 모루 오브젝트 — F키로 Anvil 씬을 Additive 로드

public class AnvilInteraction : BaseInteractable
{
    protected override void HandleInteract()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[AnvilInteraction] SceneController가 없습니다.");
            return;
        }

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null) playerInput.DeactivateInput();

        SceneController.Instance.LoadAdditive(SceneController.SceneName.Anvil);
    }
}

