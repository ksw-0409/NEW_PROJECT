using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI.Extensions;
using static UnityEditor.PlayerSettings;

public class EnemySpawner : MonoBehaviour
{
    public EnemyManager enemyManager; // 아까 만든 매니저 연결
    public Transform player;          // 플레이어 위치 기준

    [Header("설정")]
    public float spawnDistance = 10f; // 플레이어로부터 떨어진 거리
    [SerializeField] private StageManager stageManager; // 인스펙터에서 연결
    public DungeonTable table; 


    private int lastProcessedSecond = -1; //이거 스테이지 바뀔때마다 -1로 초기화 해야함
    private bool isBoss = false;
    private int[] eliteId = {2,4,6,8, -1, 10,12,14,16}; //1층부터 9층까지 엘리트몬스터 ID 목록
    private int eliteIndx = -1;
    private int currentFloor = -1;
    List<MonsterSpawnRate> targetRates; 
    private bool isInitialized = false; // 초기화 여부 체크

    [Header("돌진 패턴 설정")]
    [SerializeField] private float rushSpawnY = 12f;  // 화면 위 스폰 
    [SerializeField] private float rushDis = 24f;  // 이동거리
    public void startInit()
    {
        currentFloor = (int)GameDataManager.Instance.CurrentFloor;
        eliteIndx = currentFloor - 1;
        lastProcessedSecond = -1;
        var targetFloor = table.floors.FirstOrDefault(f => f.floorName == currentFloor);
        targetRates = targetFloor.spawnList;
        isInitialized =true;
    }
    void Update()
    {
        if ((StageManager.IsStageOver && isBoss)|| !isInitialized) return;
        float currentTime = stageManager.getTimer(); 
        int currentSecond = Mathf.FloorToInt(currentTime); //소숫점 버림
        // 1초마다 한 번씩 실행
        if (currentSecond != lastProcessedSecond)
        {
            lastProcessedSecond = currentSecond;
            HandleWaveLogic(currentSecond);
        }
    }

    void HandleWaveLogic(int sec)
    {
        //정상로직
        /*
        if (sec < 60) SpawnNormalWave(Random.Range(1,2));
        else if (sec == 60) { SpawnCircleWave(); }
        else if (sec < 120) SpawnNormalWave(Random.Range(3, 4));
        else if (sec == 120) { SpawnVerticalRush(); }
        else if (sec < 170) SpawnNormalWave(4);
        else if (sec == 180) SpawElite();
        else return;
        */
        //테스트용 로직
        if (sec == 1) Spawn(1);
        if (sec == 1) Spawn(1);
        if (sec == 1) Spawn(1);
        if (sec == 1) Spawn(1);

    }

    //일반소환
    void SpawnNormalWave(int n)
    {
        for (int i = 0; i < n; i++) {
            Spawn(GetWeightedRandom(targetRates));
        }
    }
    //각층 확률에 따른 랜덤 인덱스 반환 
    public int GetWeightedRandom(List<MonsterSpawnRate> rates)
    {
        if (rates == null || rates.Count == 0)
        {
            Debug.LogWarning("스폰 리스트가 유효하지 않습니다.");
        }
        float totalWeight = 0;
        for (int i = 0; i < rates.Count; i++)
        {
            MonsterSpawnRate rate = rates[i];
            totalWeight += rate.chance;
        }
        float pivot = Random.Range(0f, totalWeight);
        float currentSum = 0;
        for (int i = 0; i < rates.Count; i++)
        {
            MonsterSpawnRate rate = rates[i];           
            currentSum += rate.chance; // 가중치를 계속 더해나감 (구간 생성)

            if (pivot <= currentSum)
            {
                return rate.monsterID; // 구간 안에 난수가 들어오면 해당 ID 반환
            }
        }
        Debug.Log("오류로 기본 몬스터");
        return rates[0].monsterID; // 예외 처리
    }

    // 원형 접근 이벤트
    void SpawnCircleWave(int count = 36, float radius = 15.0f)
    {
        // 한 마리당 간격 각도 계산 
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            // 현재 몬스터의 각도 
            float currentAngle = i * angleStep * Mathf.Deg2Rad;
            // 원 위의 좌표 계산 (x = cos, y = sin)
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            // 플레이어 위치를 기준으로 오프셋 더하기
            Vector2 spawnPos = (Vector2)player.position + new Vector2(x, y);
            // 가중치 랜덤으로 몬스터 ID 결정 후 소환
            int monsterID = GetWeightedRandom(targetRates);
            // 몬스터 생성 (기존 Spawn 함수 활용)
            Spawn(monsterID, spawnPos);
        }
        Debug.Log("포위 스폰");
    }
    
    //3등분 밑으로 내려가는 이벤트
    public void SpawnVerticalRush()
    {
        StartCoroutine(VerticalRushRoutine());
    }
    private IEnumerator VerticalRushRoutine()
    {
        // 카메라 기준으로 가로 폭 계산 
        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize;
        float screenWidth = screenHeight * cam.aspect;
        // 3구역 중앙 X 좌표값 설정
        // -screenWidth ~ +screenWidth 사이를 3등분
        float zoneWidth = (screenWidth * 2f) / 3f;
        float[] zoneX = new float[3];
        zoneX[0] = -screenWidth + (zoneWidth * 0.5f); // 좌측 구역
        zoneX[1] = 0f;                                // 중앙 구역
        zoneX[2] = screenWidth - (zoneWidth * 0.5f);  // 우측 구역

        // 2. 3회 반복 돌진
        for (int wave = 0; wave < 3; wave++)
        {
            int randomZone = Random.Range(0, 3); // 0, 1, 2 중 랜덤
            float targetX = zoneX[randomZone];

            // 플레이어 위치를 기준으로 오프셋 더하기
            float rushYPos = player.position.y + rushSpawnY;
            Debug.Log($"[Rush] {wave + 1}차 돌진 구역: {randomZone + 1}구역");

            // 한 구역에 몬스터 여러 마리 소환 (뭉쳐서 내려오게)
            for (int i = 0; i < 10; i++)
            {
                // 약간의 가로 오프셋을 줘서 겹치지 않게
                float offsetX = Random.Range(-zoneWidth * 0.4f, zoneWidth * 0.4f);
                Vector2 spawnPos = new Vector2(targetX + offsetX, rushYPos);

                // 기존 풀링 시스템 활용 (ID는 가중치 랜덤으로 뽑기)
                int monsterID = GetWeightedRandom(targetRates);
                EnemyAI enemy = enemyManager.SpawnEnemy(monsterID, spawnPos);
                // 속도 1.4배, 체력 1.5배 버프 부여 및 하강 명령
                enemy.SetRushMode(Vector2.down, 1.4f, 1.5f, rushYPos-rushDis);
               
            }

            // 다음 돌진까지 대기 시간 (약 1.5초)
            yield return new WaitForSeconds(1.5f);
        }
    }

    //각 층수 엘리트 몬스터 소환
    void SpawElite()
    {
        int thisFloor = GameDataManager.Instance.CurrentFloor;
        if (thisFloor == 5 || thisFloor == 10) return;
         Spawn(eliteId[thisFloor - 1]);
    }

    void Spawn(int ID)
    {
        Vector2 spawnPos = GetRandomPosition();
        EnemyAI enemy = enemyManager.SpawnEnemy(ID, spawnPos);
    }
    void Spawn(int ID, Vector2 spawnPos)
    {
        EnemyAI enemy = enemyManager.SpawnEnemy(ID, spawnPos);
    }

    //생성위치 랜덤 로직
    Vector2 GetRandomPosition()
    {
        // 랜덤한 각도(0~360도) 라디안으로 계산
        float angle = Random.Range(0f, Mathf.PI * 2f);
        // 삼각함수(Cos, Sin)를 사용하면 내가 뽑은 각도가 반지름이 1인 원 위에서 어디에 위치하는지 * 거리 
        Vector2 spawnOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        return (Vector2)player.position + spawnOffset;
    }
}