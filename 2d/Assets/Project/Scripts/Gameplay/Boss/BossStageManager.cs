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

    [Header("룰렛 설정")]
    [SerializeField] private GameObject Betting;

    [Header("오버로드 설정")]
    [SerializeField] private GameObject overloadObject; // OverloadInteractable 오브젝트

    [Header("연결")]
    [SerializeField] private EnemySpawner normalSpawner;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private EnemyManager enemyManager;

    [Header("보스 체력바")]
    [SerializeField] private BossHealthBarUI bossHealthBar;

    [Header("숨길 UI")]
    [SerializeField] private GameObject stageTimerUI;

    [Header("보스 맵 경계")]
    [SerializeField] private GameObject bossStageBounds;
    [SerializeField] private SimpleInfiniteMap mapM;

    private GameObject spawnedBoss;
    private bool bossDefeated = false;
    private bool isBossStage = false;
    private int requestedBossId = 5;
    private static bool hasInitialized = false;

    void Awake()
    {
        if (hasInitialized)
        {
            Debug.Log("[BossStageManager] Awake skipped (already initialized this scene cycle)");
            return;
        }
        hasInitialized = true;

        bool requested = BossPortal.BossEntryRequested;
        BossPortal.BossEntryRequested = false;
        requestedBossId = BossPortal.RequestedBossId;

        isBossStage = requested;
        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 1;
        Debug.Log($"[BossStageManager] Awake - floor={floor} requested={requested} -> isBossStage={isBossStage}");

        if (!isBossStage)
        {
            gameObject.SetActive(false);
            return;
        }

        if (stageManager != null)
        {
            Destroy(stageManager);
            Debug.Log("[BossStageManager] StageManager DESTROYED (boss mode)");
        }
        if (normalSpawner != null)
        {
            Destroy(normalSpawner);
            Debug.Log("[BossStageManager] EnemySpawner component destroyed (boss mode)");
        }
        if (stageTimerUI != null)
        {
            stageTimerUI.SetActive(false);
            Debug.Log("[BossStageManager] StageTimerUI 숨김");
        }
        if (bossStageBounds != null)
            bossStageBounds.SetActive(true);
    }

    void OnDestroy()
    {
        hasInitialized = false;
    }

    void Start()
    {
        if (!isBossStage) return;
        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 5;
        if (mapM != null) mapM.FloorStart(floor);
        StartCoroutine(SpawnBossRoutine());
    }

    private IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(0.5f);

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
            if (spawnedBoss != null) lastBossPos = spawnedBoss.transform.position;
            yield return new WaitForSeconds(0.1f);
            var hp = spawnedBoss != null ? spawnedBoss.GetComponent<EnemyHealth>() : null;
            if (spawnedBoss == null || (hp != null && hp.currentHp <= 0))
            {
                bossDefeated = true;
                yield return StartCoroutine(BossDeathSequence(lastBossPos));

                // ✨ 경계 제거
                if (bossStageBounds != null)
                    bossStageBounds.SetActive(false);

                SpawnPortal();
                SpawnRoulette();
                SpawnOverload(); // 오버로드 오브젝트 활성화
                yield break;
            }
        }
    }

    private IEnumerator BossDeathSequence(Vector3 bossPos)
    {
        CameraShake.ShakePreset(CameraShake.Preset.Epic);
        Time.timeScale = 0.25f;
        VFXManager.SpawnBossDeath(bossPos);
        ExplodeGold(bossPos, 500);
        yield return new WaitForSecondsRealtime(0.5f);
        CameraShake.ShakePreset(CameraShake.Preset.Heavy);
        yield return new WaitForSecondsRealtime(0.7f);

        float rt = 0f;
        float from = Time.timeScale;
        while (rt < 0.4f)
        {
            rt += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(from, 1f, rt / 0.4f);
            yield return null;
        }
        Time.timeScale = 1f;

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.AddBossToken(1);

        yield return new WaitForSecondsRealtime(0.4f);
    }

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
            GoldManager.Instance.DropGold(center + (Vector3)offset, per);
        }
    }

    private void SpawnPortal()
    {
        if (player == null) { Debug.LogError("[BossStageManager] player 미할당"); return; }

        Vector3 portalPos = player.position + new Vector3(portalSpawnRadius, 0f, 0f);
        Vector3 recallPos = player.position + new Vector3(-portalSpawnRadius, 0f, 0f);

        if (requestedBossId == 5)
        {
            if (portalPrefab != null)
            {
                var portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);
                portal.GetComponent<Portal>()?.SetNextScene(SceneController.SceneName.Dungeon);
            }
            if (recallPortalPrefab != null)
                Instantiate(recallPortalPrefab, recallPos, Quaternion.identity);
            Debug.Log("[BossStageManager] 5층 보스 처치 — 다음 층 포탈 + 거점 포탈 생성");
        }
        else
        {
            if (recallPortalPrefab != null)
                Instantiate(recallPortalPrefab, portalPos, Quaternion.identity);
            Debug.Log("[BossStageManager] 10층 보스 처치 — 거점 포탈 생성");
        }
    }

    private void SpawnRoulette()
    {
        if (Betting == null || player == null)
        {
            Debug.LogError("[BossStageManager] Betting/player 미할당");
            return;
        }
        Betting.GetComponent<BettingManager>()?.Gobetting();
    }

    // 오버로드 오브젝트 활성화 및 설정
    private void SpawnOverload()
    {
        if (overloadObject == null) return;
        overloadObject.SetActive(true);
        var overload = overloadObject.GetComponent<OverloadInteractable>();
        if (overload != null)
        {
            overload.bossId = requestedBossId;
            overload.player = player;
        }
        Debug.Log($"[BossStageManager] Overload 오브젝트 활성화 (bossId={requestedBossId})");
    }
}