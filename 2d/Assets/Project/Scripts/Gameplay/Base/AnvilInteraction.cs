using UnityEngine;

// 역할: 모루 오브젝트 — E키로 아이템 강화 UI 패널 열기
//
// 유니티 세팅
//   - 모루 GameObject에 이 컴포넌트 추가
//   - anvilUI 슬롯에 아이템 강화 UI 패널 연결
public class AnvilInteraction : BaseInteractable
{
    [Header("모루 UI")]
    [SerializeField] private GameObject anvilUI;

    protected override void HandleInteract()
    {
        if (anvilUI == null)
        {
            Debug.LogWarning("[AnvilInteraction] anvilUI가 연결되지 않았습니다.");
            return;
        }

        // UI가 열려있으면 닫기, 닫혀있으면 열기 (토글)
        bool isOpen = anvilUI.activeSelf;
        anvilUI.SetActive(!isOpen);
    }
}

