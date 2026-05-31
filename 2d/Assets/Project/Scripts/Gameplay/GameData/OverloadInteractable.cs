using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 역할: 보스 처치 후 등장하는 오버로드 오브젝트

public class OverloadInteractable : BaseInteractable
{
    [Header("설정")]
    public int bossId = 5;
    public bool isSecondOverload = false;

    [Header("포탈 프리팹")]
    [SerializeField] private GameObject recallPortalPrefab;
    [SerializeField] private GameObject nextFloorPortalPrefab;
    [SerializeField] private GameObject overloadPrefab;

    [Header("스폰 설정")]
    public Transform player;
    [SerializeField] private float portalSpawnRadius = 3f;

    [Header("연결")]
    public EnemyManager enemyManager;
    public GameObject stageTimerUI;
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    public DungeonTable dungeonTable;

    [Header("몬스터 스폰 설정")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnRadius = 10f;

    private bool hasStarted = false;
    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    protected override void HandleInteract()
    {
        if (hasStarted) return;
        hasStarted = true;

        // 비주얼/콜라이더만 숨김
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        // 기존 포탈 제거
        foreach (var p in FindObjectsByType<Portal>(FindObjectsSortMode.None))
            Destroy(p.gameObject);
        foreach (var r in FindObjectsByType<RecallPortal>(FindObjectsSortMode.None))
            Destroy(r.gameObject);

        // 80% 패널티 활성화
        GameDataManager.Instance?.SetHardPenalty(true);

        // 타이머 UI 표시
        if (stageTimerUI != null)
        {
            stageTimerUI.SetActive(true);
            if (timerText == null)
                timerText = stageTimerUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        Debug.Log("[Overload] 시작");
        StartCoroutine(OverloadRoutine());
    }

    private IEnumerator OverloadRoutine()
    {
        float elapsed = 0f;
        const float duration = 90f;

        // 스폰 테이블 준비
        List<MonsterSpawnRate> spawnRates = GetSpawnRates();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 타이머 갱신
            float remaining = duration - elapsed;
            if (timerText != null)
            {
                int min = Mathf.FloorToInt(remaining / 60f);
                int sec = Mathf.FloorToInt(remaining % 60f);
                timerText.text = $"{min:00}:{sec:00}";
            }

            // 1초마다 몬스터 스폰
            if (spawnRates != null && Mathf.FloorToInt(elapsed) != Mathf.FloorToInt(elapsed - Time.deltaTime))
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
                    Vector2 spawnPos = player != null ? (Vector2)player.position + offset : offset;
                    int id = GetWeightedRandom(spawnRates);
                    var enemy = enemyManager?.SpawnEnemy(id, spawnPos);
                    if (enemy != null) enemy.SetOverloadMode(2, 2, 2);
                }
            }

            yield return null;
        }

        // 생존 성공
        GameDataManager.Instance?.SetHardPenalty(false);
        if (stageTimerUI != null) stageTimerUI.SetActive(false);
        Debug.Log("[Overload] 90초 생존 성공");
        SpawnRewards();
    }

    private List<MonsterSpawnRate> GetSpawnRates()
    {
        if (dungeonTable == null) return null;

        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 5;
        // 보스 층은 이전 층 데이터 사용
        if (floor == 5) floor = 4;
        else if (floor == 10) floor = 9;

        var targetFloor = dungeonTable.floors.FirstOrDefault(f => f.floorName == floor);
        if (targetFloor == null)
        {
            Debug.LogWarning($"[Overload] DungeonTable에 {floor}층 데이터 없음");
            return null;
        }
        return targetFloor.spawnList;
    }

    private int GetWeightedRandom(List<MonsterSpawnRate> rates)
    {
        if (rates == null || rates.Count == 0) return 0;
        float total = 0f;
        foreach (var r in rates) total += r.chance;
        float pivot = Random.Range(0f, total);
        float sum = 0f;
        foreach (var r in rates)
        {
            sum += r.chance;
            if (pivot <= sum) return r.monsterID;
        }
        return rates[0].monsterID;
    }

    private void SpawnRewards()
    {
        Transform p = player != null ? player : FindFirstObjectByType<PlayerStats>()?.transform;
        if (p == null) return;

        Vector3 recallPos = p.position + new Vector3(-portalSpawnRadius, 0f, 0f);
        Vector3 rightPos = p.position + new Vector3(portalSpawnRadius, 0f, 0f);

        if (recallPortalPrefab != null)
            Instantiate(recallPortalPrefab, recallPos, Quaternion.identity);

        if (isSecondOverload)
        {
            Debug.Log("[Overload] 두 번째 완료 — 리콜 포탈만");
            return;
        }

        if (bossId == 5)
        {
            if (nextFloorPortalPrefab != null)
            {
                var portal = Instantiate(nextFloorPortalPrefab, rightPos, Quaternion.identity);
                portal.GetComponent<Portal>()?.SetNextScene(SceneController.SceneName.Dungeon);
            }
            Debug.Log("[Overload] 5층 완료");
        }
        else if (bossId == 10)
        {
            if (overloadPrefab != null)
            {
                var newObj = Instantiate(overloadPrefab, rightPos, Quaternion.identity);
                var overload = newObj.GetComponent<OverloadInteractable>();
                if (overload != null)
                {
                    overload.bossId = 10;
                    overload.isSecondOverload = true;
                    overload.player = p;
                    overload.enemyManager = enemyManager;
                    overload.stageTimerUI = stageTimerUI;
                    overload.dungeonTable = dungeonTable;
                }
            }
            Debug.Log("[Overload] 10층 완료 — 새 오버로드");
        }
    }
}