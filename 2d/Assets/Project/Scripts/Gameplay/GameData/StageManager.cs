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
    [SerializeField] private GameObject recallPortalPrefab;
    [SerializeField] private GameObject bossPortalPrefab5;  // 5층 보스 포탈 (4층 클리어 시)
    [SerializeField] private GameObject bossPortalPrefab10; // 10층 보스 포탈 (9층 클리어 시)
    [SerializeField] private Transform player;
    [SerializeField] private SimpleInfiniteMap mapM;

    [Header("포탈 스폰 거리")]
    [SerializeField] private float portalSpawnRadius = 3f;

    private const int MAX_DUNGEON_FLOOR = 10;

    private float timer = 0f;
    private bool isStageOver = false;
    private StageData.FloorData currentFloorData;
    private PlayerSkillController skillController;

    public static event Action<float> OnTimerUpdated;
    public static bool IsStageOver { get; private set; } = false;
    public static bool IsStageActive { get; private set; } = false;

    //스테이지 오버로드용 변수
    public static bool IsOverload { get; private set; } = false;

    void OnEnable()
    {
        EnemySpawner.OnEliteKilled += OnEliteKilled;
    }

    void OnDisable()
    {
        EnemySpawner.OnEliteKilled -= OnEliteKilled;
    }

    private void OnEliteKilled()
    {
        if (isStageOver) return;
        isStageOver = true;
        StartCoroutine(StageEndRoutine());
    }

    void Start()
    {
        IsStageActive = true;
        IsStageOver = false;

        int floor = GameDataManager.Instance.CurrentFloor;
        currentFloorData = stageData.GetFloorData(floor);
        skillController = player.GetComponent<PlayerSkillController>();

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.StartTimer();

        if (floor == 5 || floor == 10)
        {
            if (enemySpawner != null) enemySpawner.gameObject.SetActive(false);
            if (mapM != null) mapM.gameObject.SetActive(false);
            SpawnBossArenaWalls(player != null ? (UnityEngine.Vector2)player.position : UnityEngine.Vector2.zero, 12f, 7f);
        }
        else
        {
            if (enemySpawner != null) enemySpawner.startInit();
            if (mapM != null) mapM.FloorStart(floor);
        }

        Debug.Log($"[StageManager] {floor}층 시작 / 제한시간: {currentFloorData.stageDuration}초");
    }

    void Update()
    {
        if (isStageOver) return;

        timer += Time.deltaTime;

        // 타이머 UI 갱신
        float remaining = Mathf.Max(0f, currentFloorData.stageDuration - timer);
        OnTimerUpdated?.Invoke(remaining);
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

        if (player != null) player.gameObject.SetActive(false);
        yield return null;
        SpawnPortal();
        if (player != null) player.gameObject.SetActive(true);
    }

    private void SpawnPortal()
    {
        if (player == null)
        {
            Debug.LogError("[StageManager] player가 없습니다.");
            return;
        }

        int floor = GameDataManager.Instance.CurrentFloor;
        Vector3 portalPos = player.position + new Vector3(portalSpawnRadius, 0f, 0f);

        // 4층 클리어 → 5층 보스 포탈
        if (floor == 4)
        {
            if (bossPortalPrefab5 == null)
            {
                Debug.LogError("[StageManager] bossPortalPrefab5가 없습니다.");
                return;
            }
            Instantiate(bossPortalPrefab5, portalPos, Quaternion.identity);
            Debug.Log("[StageManager] 4층 클리어 — 5층 보스 포탈 생성");
            return;
        }

        // 9층 클리어 → 10층 보스 포탈
        if (floor == 9)
        {
            if (bossPortalPrefab10 == null)
            {
                Debug.LogError("[StageManager] bossPortalPrefab10가 없습니다.");
                return;
            }
            Instantiate(bossPortalPrefab10, portalPos, Quaternion.identity);
            Debug.Log("[StageManager] 9층 클리어 — 10층 보스 포탈 생성");
            return;
        }

        // 일반 포탈
        if (portalPrefab == null)
        {
            Debug.LogError("[StageManager] portalPrefab이 없습니다.");
            return;
        }

        GameObject portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);
        Portal portalScript = portal.GetComponent<Portal>();
        if (portalScript != null)
        {
            string nextScene = floor >= MAX_DUNGEON_FLOOR
                ? SceneController.SceneName.Base
                : SceneController.SceneName.Dungeon;
            portalScript.SetNextScene(nextScene);
        }

        if (recallPortalPrefab != null)
        {
            Vector3 recallPos = player.position + new Vector3(-portalSpawnRadius, 0f, 0f);
            Instantiate(recallPortalPrefab, recallPos, Quaternion.identity);
        }

        Debug.Log("[StageManager] 포탈 생성 완료");
    }

    void OnDestroy() { IsStageActive = false; }

    public float getTimer() { return timer; }

    //오버로드 스테이지 시작 함수 
    private void StartOverloadWave()
    {
        if (IsOverload) return;
        IsOverload = true;
        //나머지 처리는 시간 1분30초, 층은그대로 소환로직은 스포너에서 알아서처리 
        //게임오버는 시간이 다 되었을경우에 처리하도록 변경해야함 엘리트죽을때에서 
    }

    /// <summary>보스 맵에 벽 4개를 만들어 플레이어가 너무 멀리 못 가게 함</summary>
    private void SpawnBossArenaWalls(UnityEngine.Vector2 center, float halfWidth, float halfHeight)
    {
        var parent = new GameObject("BossArenaWalls");
        parent.transform.position = center;
        // 4방향 벽 (보이지 않는 BoxCollider2D)
        SpawnWall(parent.transform, new UnityEngine.Vector2(0, halfHeight),  new UnityEngine.Vector2(halfWidth*2+1f, 1f), "Wall_Top");
        SpawnWall(parent.transform, new UnityEngine.Vector2(0, -halfHeight), new UnityEngine.Vector2(halfWidth*2+1f, 1f), "Wall_Bottom");
        SpawnWall(parent.transform, new UnityEngine.Vector2(-halfWidth, 0),  new UnityEngine.Vector2(1f, halfHeight*2+1f), "Wall_Left");
        SpawnWall(parent.transform, new UnityEngine.Vector2(halfWidth, 0),   new UnityEngine.Vector2(1f, halfHeight*2+1f), "Wall_Right");
        Debug.Log($"[StageManager] 보스 아레나 벽 생성 — 중심={center}, 크기=({halfWidth*2}, {halfHeight*2})");
    }
    private void SpawnWall(UnityEngine.Transform parent, UnityEngine.Vector2 localOffset, UnityEngine.Vector2 size, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localOffset;
        var bc = go.AddComponent<UnityEngine.BoxCollider2D>();
        bc.size = size;
        bc.isTrigger = false;
        // 살짝 보이게 (반투명 빨간선) — 빌드에서도 시각 가이드
        var sr = go.AddComponent<UnityEngine.SpriteRenderer>();
        var tex = new UnityEngine.Texture2D(1, 1);
        tex.SetPixel(0, 0, new UnityEngine.Color(0.6f, 0.1f, 0.1f, 0.25f));
        tex.Apply();
        sr.sprite = UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, 1, 1), new UnityEngine.Vector2(0.5f, 0.5f), 1f);
        sr.drawMode = UnityEngine.SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = -50;
    }
}