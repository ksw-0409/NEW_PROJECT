using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BettingManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text currentBetText; // 현재 배팅할 금액을 보여줄 텍스트
    public Button goButton; // 파칭코 시작 버튼

    // 내부 변수
    private int playerTotalGold = 0;
    private int currentBetAmount = 0;

    public GameObject Pachinko;
    public Pachinko pachinko;

    //파칭코 배팅 시작 
    void OnEnable()
    {        
        // 외부에서 플레이어의 실제 보유 골드
        playerTotalGold = GameDataManager.Instance.Gold;
        // 초기화
        ResetBet();
    }

    // +100, -100, +1000, -1000 버튼 클릭 시 호출할 함수
    public void ChangeBetAmount(int amount)
    {
        currentBetAmount += amount;

        // 배팅 금액은 0보다 작아질 수 없음
        if (currentBetAmount < 0)
        {
            currentBetAmount = 0;
        }

        // 배팅 금액은 내가 가진 총 골드를 넘길 수 없음
        if (currentBetAmount > playerTotalGold)
        {
            currentBetAmount = playerTotalGold;
        }

        UpdateUI();
    }

    // 올인 (All-In) 버튼용 함수
    public void SetAllIn()
    {
        currentBetAmount = playerTotalGold;
        UpdateUI();
    }

    // 리셋 (Reset) 버튼용 함수
    public void ResetBet()
    {
        currentBetAmount = 0;
        UpdateUI();
    }

    // 배팅 시작 (Go) 버튼용 함수
    public void StartPachinko()
    {
        this.gameObject.SetActive(false);

        if (currentBetAmount > 0 && currentBetAmount <= playerTotalGold)
        {
            Debug.Log($"파칭코 시작! 배팅된 금액: {currentBetAmount}");
            Pachinko.SetActive(true);
            pachinko.SetupBetAmount(currentBetAmount);
        }
        else
        {
            Debug.Log("배팅 금액이 0원이거나 보유 골드가 부족합니다.");
        }
    }

    // 화면 텍스트 및 버튼 상태 갱신
    private void UpdateUI()
    {
        if (currentBetText != null)
            currentBetText.text = $"{currentBetAmount}";

        // 배팅 금액이 0원이면 시작 버튼을 누르지 못하게 비활성화
        if (goButton != null)
        {
            goButton.interactable = currentBetAmount > 0;
        }
    }
}