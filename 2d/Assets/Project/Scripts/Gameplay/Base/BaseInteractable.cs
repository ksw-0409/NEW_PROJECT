using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 플레이어가 범위 안에 들어오고 E키를 누르면 상호작용하는 공통 베이스
//   - AnvilInteraction, CampfireInteraction, DungeonDoor가 이 클래스를 상속
//   - 각 자식 클래스는 HandleInteract()만 구현하면 됨

[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt;

    protected bool isPlayerInRange = false;

    private static InputControls inputControls;
    private static int refCount = 0; // 몇 개의 오브젝트가 사용 중인지 추적

    void OnEnable()
    {
        // 첫 번째 오브젝트가 활성화될 때 한 번만 생성
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

        // 모든 오브젝트가 비활성화되면 정리
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
        // ── 디버그 로그 (테스트 완료 후 삭제) ──
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