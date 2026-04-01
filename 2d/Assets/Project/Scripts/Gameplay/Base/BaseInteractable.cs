using UnityEngine;
using UnityEngine.InputSystem;

// 역할: 플레이어가 범위 안에 들어오고 E키를 누르면 상호작용하는 공통 베이스
//   - BroadcastMessage 방식 대신 InputAction을 직접 구독하는 방식 사용
//   - PlayerInput Behavior는 어떤 설정이어도 동작함
//
// 유니티 세팅
//   - 이 스크립트가 붙은 GameObject에 Collider2D (IsTrigger = true) 필수
//   - Player GameObject Tag → "Player" 설정 필수
//   - InputControls.inputactions의 Player/Interact 액션에 E키 바인딩 필수
[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt;

    protected bool isPlayerInRange = false;

    // InputAction을 직접 참조해서 구독
    private InputAction interactAction;

    void Awake()
    {
        // InputControls 에셋에서 Interact 액션 직접 가져오기
        var inputControls = new InputControls();
        interactAction = inputControls.Player.Interact;
    }

    void OnEnable()
    {
        interactAction.Enable();
        interactAction.performed += OnInteractPerformed;
    }

    void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        interactAction.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        // ── 디버그 로그 (테스트 완료 후 삭제) ──
        Debug.Log($"[OnInteract] E키 감지 / 오브젝트: {gameObject.name} / 범위 안: {isPlayerInRange}");

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