using UnityEngine;

// 역할: 플레이어가 범위 안에 들어오고 E키를 누르면 상호작용하는 공통 베이스
//   - AnvilInteraction, CampfireInteraction, DungeonDoor가 이 클래스를 상속
//   - 각 자식 클래스는 OnInteract()만 구현하면 됨
//
// 유니티 세팅
//   - 이 스크립트가 붙은 GameObject에 Collider2D(IsTrigger = true) 필수
//   - Player GameObject의 Tag를 "Player"로 설정
[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    // 상호작용 안내 UI (말풍선, "E 누르기" 텍스트 등) — 없어도 동작함
    [SerializeField] private GameObject interactPrompt;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (!isPlayerInRange) return;
        if (Input.GetKeyDown(interactKey))
        {
            OnInteract();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    // 자식 클래스에서 반드시 구현 — 실제 상호작용 동작 정의
    protected abstract void OnInteract();
}