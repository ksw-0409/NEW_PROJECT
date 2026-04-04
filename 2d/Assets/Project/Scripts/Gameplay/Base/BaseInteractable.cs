using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 플레이어가 범위 안에 들어오고 F키를 누르면 상호작용하는 공통 베이스

[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt;

    protected bool isPlayerInRange = false;
    public static bool IsUIOpen = false;

    private static InputControls inputControls;
    private static int refCount = 0;

    void OnEnable()
    {
        if (inputControls == null)
        {
            inputControls = new InputControls();
            inputControls.Player.Enable();
        }
        refCount++;
        inputControls.Player.Interact.performed += OnInteractPerformed;
    }

    void OnDisable()
    {
        inputControls.Player.Interact.performed -= OnInteractPerformed;
        refCount--;

        if (refCount <= 0)
        {
            inputControls.Player.Disable();
            inputControls.Dispose();
            inputControls = null;
            refCount = 0;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        // Canvas UI가 열려있으면 상호작용 차단
        if (IsUIOpen) return;

        Debug.Log($"[OnInteract] F키 감지 / 오브젝트: {gameObject.name} / 범위 안: {isPlayerInRange}");

        if (!isPlayerInRange) return;
        HandleInteract();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = true;
        Debug.Log($"[OnTriggerEnter2D] {gameObject.name} 범위 진입");
        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = false;
        Debug.Log($"[OnTriggerExit2D] {gameObject.name} 범위 이탈");
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    protected abstract void HandleInteract();
}