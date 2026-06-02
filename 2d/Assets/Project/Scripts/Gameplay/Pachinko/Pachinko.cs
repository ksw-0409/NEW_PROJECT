using DG.Tweening; // DOTween 사용
using UnityEngine;
using System.Collections; 

public class Pachinko : MonoBehaviour
{
    //데이터 저장용 
    public Pachinko_data items;

    //멈출 숫자 저장용
    private int[] values = new int[3];

    private int value = 0;

    //릴 조작 
    public GameObject[] Reals;

    private int savedBetAmount = 0; // 배팅 매니저가 넘겨준 금액을 임시 저장
    private bool isGameReady = false; // 레버를 당길 수 있는 상태인지 체크

    public GameObject pachin;

    [SerializeField] private GameObject goldPrefab;         // 양수(+)일 때 복수 생성할 골드 이펙트 프리팹
    [SerializeField] private GameObject explosionPrefab;    // 음수(-)일 때 복수 생성할 폭발 이펙트 프리팹

    [SerializeField] private Transform effectSpawnCenter;   // 이펙트가 생성될 중심 기준점
     private float spawnRadius = 10.0f;       // 중심점 기준 랜덤 생성할 반경 (원형 범위)

     private int maxSpawnCount = 50;        // 컴퓨터 과부하 방지를 위한 최대 생성 제한 개수

    void OnEnable()
    {
        value = 0;
        MouseManager.Instance.OpenPachinko();
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
        Invoke(nameof(RewardAndCloseRoutine), 8);
    }

    //랜덤 당첨
    private int GetRandomValue()
    {
        int value = 1;
        for (int i = 0; i < 3; i++)
        {
            int RandomIndex = UnityEngine.Random.Range(1, items.GetTotalProbability() + 1);
            int p = 0;
            for (int j = 0; j < items.items.Length; j++)
            {
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
            Reals[i].GetComponent<Pachinko_Real>().RequestStop(values[i], 2.0f + (float)i);
        }
    }

    private void RewardAndCloseRoutine()
    {
        StartCoroutine(RewardAndCloseProcess());
    }

    private IEnumerator RewardAndCloseProcess()
    {
        // 1. 모든 릴이 멈춘 이 시점에 최종 골드를 정산하여 반영합니다!
        int rewardGold = savedBetAmount * value;
        Debug.Log($"[파칭코 정산] 배팅: {savedBetAmount} x 배율: {value} = 획득: {rewardGold}");

        GameDataManager.Instance.PachinkoAddGold(rewardGold);

        // 2. 금액의 절댓값을 200으로 나눠 생성할 개수(Count) 계산
        int calculatedCount = Mathf.Abs(rewardGold) / 200;
        int finalSpawnCount = Mathf.Clamp(calculatedCount, 1, maxSpawnCount);

        if (rewardGold == 0) finalSpawnCount = 1;

        // 3. 양수/음수 조건에 따른 이펙트 종류 선택
        GameObject prefabToSpawn = (rewardGold > 0) ? goldPrefab : explosionPrefab;

        Debug.Log($"[이펙트 연출] 결과: {rewardGold} | 생성 프리팹: {prefabToSpawn.name} | 생성 개수: {finalSpawnCount}개");

        // 4. 계산된 개수만큼 랜덤 포지션에 반복 생성
        if (prefabToSpawn != null && effectSpawnCenter != null)
        {
            for (int i = 0; i < finalSpawnCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPosition = effectSpawnCenter.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                GameObject spawnedEffect = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, effectSpawnCenter);
                Destroy(spawnedEffect, 2.0f);
            }
        }

        // 5. [추가] 폭발할 때(음수일 때) 파친코 창 흔들기 효과
        if (rewardGold <= 0 && pachin != null)
        {
            // DOShakePosition(지속시간, 강도, 진동횟수)
            // 원래 위치를 지키기 위해 완전한 초기화를 보장하려면 원래 위치를 저장해두는 것이 안전합니다.
            pachin.transform.DOShakePosition(0.5f, 30f, 20, 90f, false, true);
        }

        // 6. [추가] 이펙트와 흔들림을 보여주기 위한 시간 지연 (원하는 초만큼 설정)
        // 1.5초 동안 연출을 감상한 뒤 아래 닫기 로직으로 넘어갑니다.
        yield return new WaitForSeconds(3f);

        // 사용한 배팅 금액 리셋
        savedBetAmount = 0;

        // 7. 골드 반영과 동시에 창이 솩 줄어들며 사라집니다.
        pachin.transform.DOScale(Vector3.zero, 0.4f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                pachin.SetActive(false);
                // 닫힌 후 다음 게임을 위해 스케일을 다시 1로 초기화해두는 것이 좋습니다.
                pachin.transform.localScale = Vector3.one;
            });

        // 게임이 끝났으므로 킵해둔 금액 리셋
        GameDataManager.Instance.isPachinkoActive = false;

        MouseManager.Instance.ResetToDefault();
    }
};
