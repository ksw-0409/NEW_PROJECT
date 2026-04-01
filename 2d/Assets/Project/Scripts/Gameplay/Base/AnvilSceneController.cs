using UnityEngine;
using UnityEngine.InputSystem;

// 역할: Anvil 씬 안에서 ESC키로 거점으로 복귀
//   - Anvil 씬을 Unload하면 Base 씬이 그대로 남아있음

public class AnvilSceneController : MonoBehaviour
{
    private InputAction escAction;

    void Awake()
    {
        escAction = new InputAction(binding: "<Keyboard>/escape");
    }

    void OnEnable()
    {
        escAction.Enable();
        escAction.performed += OnEscPressed;
    }

    void OnDisable()
    {
        escAction.performed -= OnEscPressed;
        escAction.Disable();
    }

    private void OnEscPressed(InputAction.CallbackContext ctx)
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[AnvilSceneController] SceneController�� �����ϴ�.");
            return;
        }

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null) playerInput.ActivateInput();

        SceneController.Instance.UnloadAdditive(SceneController.SceneName.Anvil);
    }
}
