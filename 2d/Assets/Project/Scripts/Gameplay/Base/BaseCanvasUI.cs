using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 거점 Canvas UI 공통 베이스

public abstract class BaseCanvasUI : MonoBehaviour
{
    private InputAction escAction;
    private PlayerInput playerInput;

    void Awake()
    {
        escAction = new InputAction(binding: "<Keyboard>/escape");
        OnAwake();
    }

    void OnEnable()
    {
        BaseInteractable.IsUIOpen = true;

        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null) playerInput.DeactivateInput();

        escAction.Enable();
        escAction.performed += OnEscPressed;

        OnOpen();
    }

    void OnDisable()
    {
        escAction.performed -= OnEscPressed;
        escAction.Disable();

        BaseInteractable.IsUIOpen = false;

        if (playerInput != null) playerInput.ActivateInput();

        OnClose();
    }

    private void OnEscPressed(InputAction.CallbackContext ctx)
    {
        if (!CanClose()) return;
        gameObject.SetActive(false);
    }

    protected virtual void OnAwake() { }   // Awake 시 추가 초기화
    protected virtual void OnOpen() { }    // Canvas 열릴 때
    protected virtual void OnClose() { }   // Canvas 닫힐 때

    protected virtual bool CanClose() => true;
}