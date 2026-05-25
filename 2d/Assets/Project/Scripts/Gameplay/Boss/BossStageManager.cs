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
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float portalSpawnRadius = 3f;

    [Header("✨ 룰렛 설정")]
    [SerializeField] private GameObject roulettePrefab;
    [SerializeField] private float rouletteSpawnRadius = 5f;

    [Header("연결")]
    [SerializeField] private EnemySpawner normalSpawner;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private EnemyManager enemyManager;

    private GameObject spawnedBoss;
    private bool bossDefeated = false;
    private bool isBossStage = false;
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

        if (bossPrefab == null)
        {
            Debug.LogError("[BossStageManager] bossPrefab 미할당");
            yield break;
        }

        if (enemyManager != null && enemyManager.player == null && player != null)
        {
            enemyManager.player = player;
            Debug.Log("[BossStageManager] EnemyManager.player auto-assigned");
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : new Vector3(0f, 4f, 0f);
        spawnedBoss = Instantiate(bossPrefab, pos, Quaternion.identity);
        Debug.Log($"[BossStageManager] Boss spawned at {pos}");

        var ai = spawnedBoss.GetComponent<EnemyAI>();
        if (ai != null) ai.Init();
        if (enemyManager != null && ai != null)
        {
            enemyManager.activeEnemies.Add(ai);
            Debug.Log("[BossStageManager] Boss registered in EnemyManager");
        }

        StartCoroutine(WatchBossDeath());
    }

    private IEnumerator WatchBossDeath()
    {
        while (spawnedBoss != null && !bossDefeated)
        {
            yield return new WaitForSeconds(0.3f);
            var hp = spawnedBoss != null ? spawnedBoss.GetComponent<EnemyHealth>() : null;
            if (spawnedBoss == null || (hp != null && hp.currentHp <= 0))
            {
                bossDefeated = true;
                yield return new WaitForSeconds(1.5f);
                SpawnPortal();
                SpawnRoulette();
                yield break;
            }
        }
    }

    private void SpawnPortal()
    {
        if (portalPrefab == null || player == null)
        {
            Debug.LogError("[BossStageManager] portalPrefab/player 미할당");
            return;
        }
        Vector3 portalPos = player.position + new Vector3(portalSpawnRadius, 0f, 0f);
        GameObject portal = Instantiate(portalPrefab, portalPos, Quaternion.identity);
        Portal ps = portal.GetComponent<Portal>();
        if (ps != null) ps.SetNextScene(SceneController.SceneName.Base);
        Debug.Log("[BossStageManager] 보스 처치 — 베이스로 가는 포탈 생성");
    }
    private void SpawnRoulette()
    {
        if (roulettePrefab == null || player == null)
        {
            Debug.LogError("[BossStageManager] roulettePrefab/player 미할당");
            return;
        }
        Vector3 roulettePos = player.position + new Vector3(-rouletteSpawnRadius, 0f, 0f);
        Instantiate(roulettePrefab, roulettePos, Quaternion.identity);
        Debug.Log("[BossStageManager] 룰렛 오브젝트 생성");
    }
}
