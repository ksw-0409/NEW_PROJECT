using UnityEngine;

// 역할: 모닥불 오브젝트 — E키로 스킬 선택 UI 패널 열기
//
// 유니티 세팅
//   - 모닥불 GameObject에 이 컴포넌트 추가
//   - campfireUI 슬롯에 스킬 선택 UI 패널 연결
public class CampfireInteraction : BaseInteractable
{
    [Header("모닥불 UI")]
    [SerializeField] private GameObject campfireUI;

    protected override void OnInteract()
    {
        if (campfireUI == null)
        {
            Debug.LogWarning("[CampfireInteraction] campfireUI가 연결되지 않았습니다.");
            return;
        }

        bool isOpen = campfireUI.activeSelf;
        campfireUI.SetActive(!isOpen);
    }
}
