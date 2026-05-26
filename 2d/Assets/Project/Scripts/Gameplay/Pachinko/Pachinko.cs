using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using DG.Tweening; // DOTween 사용

public class Pachinko : MonoBehaviour
{
    //데이터 저장용 
    public Pachinko_data items;

    //멈출 숫자 저장용
    private int[] values = new int[3];

    private int value=0;

    //릴 조작 
    public GameObject[] Reals;

    private int savedBetAmount = 0; // 배팅 매니저가 넘겨준 금액을 임시 저장
    private bool isGameReady = false; // 레버를 당길 수 있는 상태인지 체크

    public GameObject pachin;
    void OnEnable()
    {
        value = 0;
        for (int i = 0; i < 3; i++) values[i] = 0;
    }

    public void SetupBetAmount(int betAmount)
    {
        savedBetAmount = betAmount;
        // 데이터 매니저에 골드 반영
        GameDataManager.Instance.PachinkoAddGold(-savedBetAmount);
        isGameReady = true;
        Debug.Log($"[파칭코 기계] {savedBetAmount}골드 세팅 완료! 레버를 당겨주세요.");
    }

    public void StartPachinko()
    {
        if (!isGameReady) return;
        isGameReady = false; // 중복 실행 방지
        StartReal(); 
        value = GetRandomValue();
        StopAllReels();
        // 릴이 다 멈추는 시간 에 정산 및 닫기 연출 시작
        Invoke(nameof(RewardAndCloseRoutine), 10);
    }

    //랜덤 당첨
    private int GetRandomValue()
    {
        int value = 1;
        for (int i = 0; i < 3; i++)
        {
            int RandomIndex = UnityEngine.Random.Range(1, items.GetTotalProbability()+1);
            int p = 0;
            for (int j = 0; j < items.items.Length; j++) {
                p += items.items[j].probability;
                if (RandomIndex <= p)
                {
                    value *= items.items[j].itemValue;
                    values[i] = items.items[j].itemValue;
                    Debug.Log(value);
                    break;
                } 
            }
        }
        Debug.Log(value);
        return value;
    }

    private void StartReal()
    {
        for (int i = 0; i < Reals.Length; i++)
        {
            Reals[i].GetComponent<Pachinko_Real>().StartSpin();
        }
    }
    private void StopAllReels()
    {
        for (int i = 0; i < Reals.Length; i++)
        {
            Reals[i].GetComponent<Pachinko_Real>().RequestStop(values[i], 2.0f+ (float)i);
        }
    }

    private void RewardAndCloseRoutine()
    {
        // 1. 모든 릴이 멈춘 이 시점에 최종 골드를 정산하여 반영합니다!
        int rewardGold = savedBetAmount * value;
        Debug.Log($"[파칭코 정산] 배팅: {savedBetAmount} x 배율: {value} = 획득: {rewardGold}");

        GameDataManager.Instance.PachinkoAddGold(rewardGold);

        // 사용한 배팅 금액 리셋
        savedBetAmount = 0;

        // 2. 골드 반영과 동시에 창이 솩 줄어들며 사라집니다.
        pachin.transform.DOScale(Vector3.zero, 0.4f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                pachin.SetActive(false);
            });
        // 게임이 끝났으므로 킵해둔 금액 리셋
        savedBetAmount = 0;
        GameDataManager.Instance.isPachinkoActive = false;
    }
}
