using System.Collections;
using UnityEditor.Experimental.GraphView;
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

    void Start()
    {
        int floor = GameDataManager.Instance.CurrentFloor;
        currentFloorData = stageData.GetFloorData(floor);

        skillController = player.GetComponent<PlayerSkillController>();

        if (floor == 5 || floor == 10)
        {
            if (enemySpawner != null)
                enemySpawner.gameObject.SetActive(false);
        }

        Debug.Log($"[StageManager] {floor}층 시작 / 제한시간: {currentFloorData.stageDuration}초");
    }

    void Update()
    {
        if (isStageOver) return;

        timer += Time.deltaTime;

        if (timer >= currentFloorData.stageDuration)
            StartCoroutine(StageEndRoutine());
    }

    private IEnumerator StageEndRoutine()
    {
        isStageOver = true;

        int floor = GameDataManager.Instance.CurrentFloor;
        Debug.Log($"[StageManager] {floor}층 클리어");

        if (enemyManager != null)
            enemyManager.gameObject.SetActive(false);

        // 플레이어 오브젝트 비활성화 -> 모든 코루틴 강제 종료
        if (player != null)
            player.gameObject.SetActive(false);

        yield return null;

        // 포탈 생성 후 플레이어 다시 활성화
        SpawnPortal();

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
}