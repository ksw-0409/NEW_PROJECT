using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BettingManager : MonoBehaviour
{
    [SerializeField] private GameObject bettingPanel; 

    private bool isPaused = false;
    private bool isPlusMode = true; 

    [Header("UI 연결")]
    public TMP_Text currentBetText;
    public Button goButton;
    public Button PButton;
    public Button MButton;

    [Header("스프라이트 배열 (0: 비활성/기본, 1: 활성/눌림)")]
    public Sprite[] Pbs;
    public Sprite[] Mbs;

    // 내부 변수
    private int playerTotalGold = 0;
    private int currentBetAmount = 0;

    [Header("파칭코 시스템 연결")]
    public GameObject Pachinko;
    public Pachinko pachinko;

    // 파칭코 배팅 창이 켜질 때
    void OnEnable()
    {
        setUp();
        GameDataManager.Instance.isPachinkoActive = true;

        // 보유 골드 최신화
        playerTotalGold = GameDataManager.Instance.Gold;

        // 초기화
        ResetBet();
    }

    // +100, -100(인스펙터에는 양수 100, 1000 입력 권장) 버튼 클릭 시 호출
    public void ChangeBetAmount(int amount)
    {
        // 항상 매개변수는 양수로 받고, 마이너스 모드일 때만 음수로 변환하는 것이 안전함
        int finalAmount = Mathf.Abs(amount);

        if (!isPlusMode)
        {
            finalAmount = -finalAmount;
        }

        currentBetAmount += finalAmount;

        // 배팅 금액 한계 제한 (0 ~ 보유 골드)
        currentBetAmount = Mathf.Clamp(currentBetAmount, 0, playerTotalGold);

        UpdateUI();
    }

    // 올인 (All-In)
    public void SetAllIn()
    {
        currentBetAmount = playerTotalGold;
        UpdateUI();
    }

    // 리셋 (Reset)
    public void ResetBet()
    {
        currentBetAmount = 0;
        UpdateUI();
    }

    // 플러스 모드 설정
    public void setUp()
    {
        isPlusMode = true;
        if (PButton != null && Pbs.Length > 1) PButton.image.sprite = Pbs[1];
        if (MButton != null && Mbs.Length > 0) MButton.image.sprite = Mbs[0];
    }

    // 마이너스 모드 설정
    public void setM()
    {
        isPlusMode = false;
        if (PButton != null && Pbs.Length > 0) PButton.image.sprite = Pbs[0];
        if (MButton != null && Mbs.Length > 1) MButton.image.sprite = Mbs[1];
    }

    // 배팅 시작 (Go) 버튼
    public void StartPachinko()
    {
        // [수정] 조건 검사를 먼저 한 뒤에 창을 꺼야 버그가 안 생깁니다.
        if (currentBetAmount > 0 && currentBetAmount <= playerTotalGold)
        {
            Debug.Log($"파칭코 시작! 배팅된 금액: {currentBetAmount}");

            this.gameObject.SetActive(false); // 성공했을 때만 배팅창 끄기
            Pachinko.SetActive(true);
            pachinko.SetupBetAmount(currentBetAmount);
        }
        else
        {
            Debug.LogError("배팅 금액이 0원이거나 보유 골드가 부족합니다.");
        }
    }

    public void exitButtn()
    {
        GameDataManager.Instance.isPachinkoActive = false;
        this.gameObject.SetActive(false);
    }

    // UI 갱신
    private void UpdateUI()
    {
        if (currentBetText != null)
            currentBetText.text = $"{currentBetAmount}";

        if (goButton != null)
        {
            // 0원일 때는 버튼 클릭 불가능하게 방어 코드
            goButton.interactable = currentBetAmount > 0;
        }
    }

    // 배팅창 토글 켜고 끄기
    public void Gobetting()
    {
        isPaused = !isPaused;
        if (bettingPanel != null) bettingPanel.SetActive(isPaused);
    }
}