using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 스테이지 매니저 — BossPortal로 진입한 경우만 동작.
/// 일반 DungeonDoor 진입 시에는 자기 자신만 비활성화하고 기존 게임 흐름을 절대 건드리지 않음.
/// </summary>
[DefaultExecutionOrder(-100)]
public class BossStageManager : MonoBehaviour
{
    [Header("보스 설정")]
    [SerializeField] private GameObject bossPrefab;          // 5층 보스 (실바누스)
    [SerializeField] private GameObject voidPriestPrefab;    // 10층 보스 (공허의 대사제)
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject portalPrefab;       // 다음 층 포탈
    [SerializeField] private GameObject recallPortalPrefab; // 거점 귀환 포탈
    [SerializeField] private float portalSpawnRadius = 3f;

    [Header("✨ 룰렛 설정")]
    [SerializeField] private GameObject Betting;

    [Header("연결")]
    [SerializeField] private EnemySpawner normalSpawner;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private EnemyManager enemyManager;

    [Header("보스 체력바")]
    [SerializeField] private BossHealthBarUI bossHealthBar;

    [Header("숨길 UI")]
    [SerializeField] private GameObject stageTimerUI;

    private GameObject spawnedBoss;
    private bool bossDefeated = false;
    private bool isBossStage = false;
    private int requestedBossId = 5; // 진입 시 요청된 보스 ID (5=실바누스, 10=공허의 대사제);
    private static bool hasInitialized = false; // ⭐ 이중 초기화 방지

    void Awake()
    {
        // 같은 씬 진입에서 두 번째 Awake가 호출되더라도 두 번 처리하지 않음
        if (hasInitialized)
        {
            Debug.Log("[BossStageManager] Awake skipped (already initialized this scene cycle)");
            return;
        }
        hasInitialized = true;

        bool requested = BossPortal.BossEntryRequested;
        BossPortal.BossEntryRequested = false; // 즉시 소비
        requestedBossId = BossPortal.RequestedBossId; // 요청된 보스 ID 저장 // 즉시 소비

        isBossStage = requested;
        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 1;
        Debug.Log($"[BossStageManager] Awake - floor={floor} requested={requested} -> isBossStage={isBossStage}");

        if (!isBossStage)
        {
            // 일반 진입: 자기 자신만 비활성화
            gameObject.SetActive(false);
            return;
        }

        // 보스 진입: StageManager와 EnemySpawner 자체를 즉시 파괴 (재활성화 불가능하도록)
        if (stageManager != null)
        {
            Destroy(stageManager.gameObject);
            Debug.Log("[BossStageManager] StageManager DESTROYED (boss mode)");
        }
        if (normalSpawner != null)
        {
            // EnemySpawner는 EnemyManager와 같은 GameObject에 있을 수 있으니 컴포넌트만 파괴
            Destroy(normalSpawner);
            Debug.Log("[BossStageManager] EnemySpawner component destroyed (boss mode)");
        }

        if (stageTimerUI != null)
        {
            stageTimerUI.SetActive(false);
            Debug.Log("[BossStageManager] StageTimerUI 숨김");
        }
    }

    void OnDestroy()
    {
        // 씬을 떠날 때 정적 플래그 리셋
        hasInitialized = false;
    }

    void Start()
    {
        if (!isBossStage) return;
        StartCoroutine(SpawnBossRoutine());
    }

    private IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        // 요청된 보스 ID에 따라 스폰할 프리팹 선택 (10 = 공허의 대사제, 그 외 = 기본 보스)
        GameObject selectedBoss = (requestedBossId == 10 && voidPriestPrefab != null) ? voidPriestPrefab : bossPrefab;
        Debug.Log($"[BossStageManager] requestedBossId={requestedBossId} -> spawn {(selectedBoss != null ? selectedBoss.name : "NULL")}");

        if (selectedBoss == null)
        {
            Debug.LogError("[BossStageManager] 스폰할 보스 프리팹 미할당 (bossId=" + requestedBossId + ")");
            yield break;
        }

        if (enemyManager != null && enemyManager.player == null && player != null)
        {
            enemyManager.player = player;
            Debug.Log("[BossStageManager] EnemyManager.player auto-assigned");
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : new Vector3(0f, 4f, 0f);
        spawnedBoss = Instantiate(selectedBoss, pos, Quaternion.identity);
        Debug.Log($"[BossStageManager] Boss spawned at {pos}");

        var ai = spawnedBoss.GetComponent<EnemyAI>();
        if (ai != null) ai.Init();
        if (enemyManager != null && ai != null)
        {
            enemyManager.activeEnemies.Add(ai);
            Debug.Log("[BossStageManager] Boss registered in EnemyManager");
        }

        if (bossHealthBar != null)
        {
            var bossHp = spawnedBoss.GetComponent<EnemyHealth>();
            if (bossHp != null) bossHealthBar.SetTarget(bossHp);
        }

        StartCoroutine(WatchBossDeath());
    }

    private IEnumerator WatchBossDeath()
    {
        Vector3 lastBossPos = spawnedBoss != null ? spawnedBoss.transform.position : Vector3.zero;
        while (spawnedBoss != null && !bossDefeated)
        {
            // 보스가 살아있는 동안 계속 마지막 위치 기록 (죽어서 Destroy돼도 이 위치 사용)
            if (spawnedBoss != null) lastBossPos = spawnedBoss.transform.position;

            yield return new WaitForSeconds(0.1f);
            var hp = spawnedBoss != null ? spawnedBoss.GetComponent<EnemyHealth>() : null;
            if (spawnedBoss == null || (hp != null && hp.currentHp <= 0))
            {
                bossDefeated = true;

                // 머지막으로 기록된 보스 위치 사용 (Destroy 이후에도 안전)
                Vector3 bossPos = lastBossPos;

                // 화려한 처치 연출 (슬로우 + 폭죽 + 흔들림 + 골드 폭발) — 끝나면 증표 지급
                yield return StartCoroutine(BossDeathSequence(bossPos));

                SpawnPortal();
                //룰렛 상호작용 룰렛 
                //함수 상호작용 밑에꺼  
                SpawnRoulette();
                yield break;
            }
        }
    }

    // 🎆 보스 처치 연출 시퀀스 (슬로우 + 폭죽 + 화면흔들림 + 골드 폭발)
    private IEnumerator BossDeathSequence(Vector3 bossPos)
    {
        // 1) 순간 강한 화면 흔들림
        CameraShake.ShakePreset(CameraShake.Preset.Epic);

        // 2) 시간을 천천히 (슬로우 모션) — 이펙트도 같이 느려져 폭죽이 천천히 터짐
        Time.timeScale = 0.25f;

        // 3) 보스 폭죽 이펙트
        VFXManager.SpawnBossDeath(bossPos);

        // 4) 골드 500 사방 폭발 산개
        ExplodeGold(bossPos, 500);

        // 슬로우 유지 (실시간 기준 — timeScale 영향 안 받음)
        yield return new WaitForSecondsRealtime(0.5f);
        CameraShake.ShakePreset(CameraShake.Preset.Heavy); // 두 번째 여진
        yield return new WaitForSecondsRealtime(0.7f);

        // 5) 시간을 부드럽게 원복
        float rt = 0f;
        float from = Time.timeScale;
        while (rt < 0.4f)
        {
            rt += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(from, 1f, rt / 0.4f);
            yield return null;
        }
        Time.timeScale = 1f;

        // 6) 보스 처치 증표 지급 (+1)
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.AddBossToken(1);

        yield return new WaitForSecondsRealtime(0.4f);
    }

    // 골드를 여러 덩어리로 사방에 터트려 떨어뜨린다
    private void ExplodeGold(Vector3 center, int totalGold)
    {
        if (GoldManager.Instance == null)
        {
            if (GameDataManager.Instance != null) GameDataManager.Instance.AddGold(totalGold);
            return;
        }

        int chunks = 20;
        int per = Mathf.Max(1, totalGold / chunks);
        for (int i = 0; i < chunks; i++)
        {
            float ang = (i / (float)chunks) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            float dist = Random.Range(1.2f, 3.5f);
            Vector2 offset = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist;
            Vector3 dropPos = center + (Vector3)offset;
            GoldManager.Instance.DropGold(dropPos, per);
        }
    }

    private void SpawnPortal()
    {
        if (player == null)
        {
            Debug.LogError("[BossStageManager] player 미할당");
            return;
        }

        Vector3 portalPos = player.position + new Vector3(portalSpawnRadius, 0f, 0f);
        Vector3 recallPortalPos = player.position + new Vector3(-portalSpawnRadius, 0f, 0f);

        if (requestedBossId == 5)
        {
            // 5층 보스 처치 → 다음 층 포탈 + 거점 귀환 포탈
            if (portalPrefab != null)
            {
                var portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);
                portal.GetComponent<Portal>()?.SetNextScene(SceneController.SceneName.Dungeon);
            }
            if (recallPortalPrefab != null)
                Instantiate(recallPortalPrefab, recallPortalPos, Quaternion.identity);

            Debug.Log("[BossStageManager] 5층 보스 처치 — 다음 층 포탈 + 거점 포탈 생성");
        }
        else
        {
            // 10층 보스 처치 → 거점 귀환 포탈만
            if (recallPortalPrefab != null)
                Instantiate(recallPortalPrefab, portalPos, Quaternion.identity);

            Debug.Log("[BossStageManager] 10층 보스 처치 — 거점 포탈 생성");
        }
    }


    private void SpawnRoulette()
    {
        if (Betting == null || player == null)
        {
            Debug.LogError("[BossStageManager] roulettePrefab/player 미할당");
            return;
        }
        Betting.GetComponent<BettingManager>().Gobetting();
    }
}