using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("튜토리얼 페이지 프리팹/오브젝트 배열")]
    public GameObject[] tutorialPages; // 1페이지, 2페이지 오브젝트를 순서대로 등록

    [Header("이동 버튼들")]
    public Button prevButton; // 이전 버튼
    public Button nextButton; // 다음 버튼 (마지막 장에서는 완료 버튼 역할)

    private int currentIndex = 0; // 현재 보고 있는 페이지 번호

    void Start()
    {
        // 게임 시작 시 첫 페이지 세팅
        ShowPage(0);
    }

    // 핵심: 원하는 인덱스의 페이지는 켜고, 나머지는 싹 다 끄는 함수
    public void ShowPage(int index)
    {
        // 예외 방어 (배열이 비어있으면 실행 안 함)
        if (tutorialPages == null || tutorialPages.Length == 0) return;

        currentIndex = index;

        // 1. 모든 페이지를 순회하면서 현재 번호만 활성화
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].SetActive(i == currentIndex);
            }
        }

        // 2. [이전] 버튼 제어: 0번째(첫 번째) 페이지면 이전 버튼 숨기기/비활성화
        prevButton.gameObject.SetActive(currentIndex > 0);

        // 3. [다음] 버튼 텍스트 제어
        TMPro.TextMeshProUGUI nextText = nextButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nextText != null)
        {
            // 마지막 페이지라면 버튼 글자를 "완료"나 "닫기"로 변경
            if (currentIndex == tutorialPages.Length - 1)
            {
                nextText.text = "완료";
            }
            else
            {
                nextText.text = "다음";
            }
        }
    }

    // [다음] 버튼을 눌렀을 때 실행할 함수
    public void OnClickNext()
    {
        // 아직 뒤에 페이지가 더 남아있다면 다음 장으로
        if (currentIndex < tutorialPages.Length - 1)
        {
            ShowPage(currentIndex + 1);
        }
        else
        {
            // 마지막 장에서 다음을 누르면 튜토리얼 창 전체를 비활성화(닫기)
            CloseTutorial();
        }
    }

    // [이전] 버튼을 눌렀을 때 실행할 함수
    public void OnClickPrev()
    {
        if (currentIndex > 0)
        {
            ShowPage(currentIndex - 1);
        }
    }

    // 튜토리얼 종료 함수
    private void CloseTutorial()
    {
        Debug.Log("튜토리얼 종료!");
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(false); 
        }
        gameObject.SetActive(false); // 매니저 오브젝트 자체를 꺼버림
    }
}