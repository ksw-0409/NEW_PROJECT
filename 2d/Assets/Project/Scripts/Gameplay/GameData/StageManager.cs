using System;
using System.Collections;
using UnityEngine;

// 역할: 스테이지 전체 흐름 제어

public class StageManager : MonoBehaviour
{
    [Header("스테이지 설정")]
    [SerializeField] private StageData stageData;

    [Header("연결")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform player;

    [Header("포탈 스폰 거리")]
    [SerializeField] private float portalSpawnRadius = 3f;

    private const int MAX_DUNGEON_FLOOR = 10;

    private float timer = 0f;
    private bool isStageOver = false;
    private StageData.FloorData currentFloorData;
    private PlayerSkillController skillController;

    // 타이머 UI에 남은 시간 전달
    public static event Action<float> OnTimerUpdated;
    public static bool IsStageOver { get; private set; } = false;
    public static bool IsStageActive { get; private set; } = false;

    void Start()
    {
        IsStageActive = true;
        IsStageOver = false;

        int floor = GameDataManager.Instance.CurrentFloor;
        currentFloorData = stageData.GetFloorData(floor);
        skillController = player.GetComponent<PlayerSkillController>();

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.StartTimer();
        
        // 5층, 10층은 보스 층 — 일반 몬스터 스폰 비활성화
        if (floor == 5 || floor == 10)
        {
            if (enemySpawner != null)
                enemySpawner.gameObject.SetActive(false);
        }
        else
        {
            // 보스층이 아닐 때만 안전하게 일반 몬스터 데이터 초기화 실행
            if (enemySpawner != null)
            {
                enemySpawner.startInit();
            }
        }
        Debug.Log($"[StageManager] {floor}층 시작 / 제한시간: {currentFloorData.stageDuration}초");
    }

    void Update()
    {
        if (isStageOver) return;

        timer += Time.deltaTime;

        // 남은 시간 계산 후 이벤트 발생
        float remaining = Mathf.Max(0f, currentFloorData.stageDuration - timer);
        OnTimerUpdated?.Invoke(remaining);

        if (timer >= currentFloorData.stageDuration && !isStageOver)
        {
            isStageOver = true;
            StartCoroutine(StageEndRoutine());
        }
    }

    private IEnumerator StageEndRoutine()
    {
        IsStageOver = true;
        int floor = GameDataManager.Instance.CurrentFloor;
        Debug.Log($"[StageManager] {floor}층 클리어");

        if (enemyManager != null)
        {
            enemyManager.gameObject.SetActive(false);
            Debug.Log("[StageManager] 적 제거 완료");
        }

        // 플레이어 비활성화 -> 스킬 코루틴 강제 종료
        if (player != null)
            player.gameObject.SetActive(false);

        yield return null;

        SpawnPortal();

        // 플레이어 재활성화
        if (player != null)
            player.gameObject.SetActive(true);
    }

    private void SpawnPortal()
    {
        if (portalPrefab == null || player == null)
        {
            Debug.LogError("[StageManager] portalPrefab 또는 player가 없습니다.");
            return;
        }

        Vector3 portalPos = player.position + new Vector3(portalSpawnRadius, 0f, 0f);
        GameObject portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);

        Portal portalScript = portal.GetComponent<Portal>();
        if (portalScript != null)
        {
            int floor = GameDataManager.Instance.CurrentFloor;
            string nextScene = floor >= MAX_DUNGEON_FLOOR
                ? SceneController.SceneName.Base
                : SceneController.SceneName.Dungeon;

            portalScript.SetNextScene(nextScene);
        }

        Debug.Log("[StageManager] 포탈 생성 완료");
    }

    void OnDestroy()
    {
        IsStageActive = false;
    }

    public float getTimer() { return timer; }   
}