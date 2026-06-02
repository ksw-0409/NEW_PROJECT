using UnityEngine;

// 역할: 거점 최초 진입 시 튜토리얼 패널 표시
// tr == 0: 튜토리얼 표시 후 tr = 1로 변경
// tr == 1: 튜토리얼 오브젝트 삭제

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] private GameObject tutorialObject;

    void Start()
    {
        if (GameDataManager.Instance == null) return;

        if (GameDataManager.Instance.tr == 0)
        {
            // 최초 방문 — 튜토리얼 표시 후 완료 처리
            if (tutorialObject != null) tutorialObject.SetActive(true);
            GameDataManager.Instance.tr = 1;
        }
        else
        {
            // 이미 봤음 — 삭제
            if (tutorialObject != null) Destroy(tutorialObject);
        }
    }
}