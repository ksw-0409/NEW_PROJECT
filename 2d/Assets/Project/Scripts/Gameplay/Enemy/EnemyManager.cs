using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; } // 편의를 위한 싱글톤
    public Transform player;
    private IObjectPool<EnemyAI> pool;
    public List<EnemyAI> activeEnemies = new List<EnemyAI>();   // 현재 활성화되어 움직여야 할 적들을 따로 관리하는 리스트

    // 죽은 몬스터들을 임시로 담아둘 큐 변수
    private Queue<EnemyAI> enemiesToRelease = new Queue<EnemyAI>();

    // ID를 주면 오브젝트 풀 자판기를 아웃해주는 딕셔너리 (잡몹용)
    private Dictionary<int, IObjectPool<EnemyAI>> poolDict = new Dictionary<int, IObjectPool<EnemyAI>>();

    // ID를 주면 EnemyAI 프리팹 원본을 바로 아웃해주는 딕셔너리 (정예몹/잡몹 공용)
    private Dictionary<int, EnemyAI> enemyPrefabsDict = new Dictionary<int, EnemyAI>();

    [Header("다양한 몬스터 프리팹 리스트")]
    public List<EnemyAI> enemyPrefabs;

    [Header("게임 통계 (Statistics)")]
    public int totalKillCount { get; private set; } = 0;
    public float totalDamageDealt { get; private set; } = 0f;

    void Awake()
    {
        Instance = this; // 싱글톤 초기화
                         // 딕셔너리 주입 및 풀 생성 단계
        foreach (var prefab in enemyPrefabs)
        {
            int currentID = prefab.GetID();
            EnemyAI prefabRef = prefab;
            // 쌩 프리팹 원본을 뱉어주는 딕셔너리에 무조건 먼저 등록 (정예몹 소환 차단 방지)
            if (!enemyPrefabsDict.ContainsKey(currentID))
            {
                enemyPrefabsDict.Add(currentID, prefabRef);
            }
            else
            {
                Debug.LogError($"중복된 몬스터 ID 발견: {prefab.name}. 프리팹 확인이 필요합니다.");
                continue;
            }
            // 풀링을 쓰지 않는 정예몹 유형이라면 풀 자판기 생성을 건너뜀
            if (!prefabRef.usePooling) continue;
            // 풀링을 쓰는 잡몹 유형만 오브젝트 풀 생성 및 등록
            ObjectPool<EnemyAI> pool = null;
            pool = new ObjectPool<EnemyAI>(
                createFunc: () => {
                    EnemyAI ai = Instantiate(prefabRef, transform);
                    ai.SetPool(pool);
                    return ai;
                },
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                actionOnDestroy: OnDestroyEnemy,
                maxSize: 100
            );

            poolDict.Add(currentID, pool);
        }

        totalKillCount = 0;
        totalDamageDealt= 0f;   
    }

    private void OnGetEnemy(EnemyAI ai)
    {
        ai.gameObject.SetActive(true);
        ai.Init();
        activeEnemies.Add(ai); // 업데이트 루프를 위해 활성 리스트에 추가
    }

    private void OnReleaseEnemy(EnemyAI ai)
    {
        ai.gameObject.SetActive(false);
        activeEnemies.Remove(ai); // 활성 리스트에서 제거
    }

    private void OnDestroyEnemy(EnemyAI ai)
    {
        Destroy(ai.gameObject);
    }

    public EnemyAI SpawnEnemy(int monsterID, Vector3 position)
    {
        if (!enemyPrefabsDict.TryGetValue(monsterID, out EnemyAI prefab))
        {
            Debug.LogError($"[SpawnEnemy] 존재하지 않는 몬스터 ID입니다: {monsterID}");
            return null;
        }
        EnemyAI spawnedEnemy = null;
        // 아웃받은 프리팹의 플래그를 확인하여 스폰 분기 처리
        if (prefab.usePooling)
        {
            // 잡몹 방식: 풀 딕셔너리에서 자판기를 꺼내 Get() (OnGetEnemy가 자동 실행됨)
            if (poolDict.TryGetValue(monsterID, out var enemyPool))
            {
                spawnedEnemy = enemyPool.Get();
            }
        }
        else
        {
            // 정예몹 방식: 풀을 쓰지 않으므로 프리팹 원본 기반으로 즉시 생성
            spawnedEnemy = Instantiate(prefab, transform);
            spawnedEnemy.Init();
            activeEnemies.Add(spawnedEnemy); // 정예몹은 풀을 안 거치므로 리스트에 수동 추가
        }

        if (spawnedEnemy != null)
        {
            spawnedEnemy.transform.position = position;
        }

        return spawnedEnemy;
    }

    public void EnqueueToRelease(EnemyAI ai)
    {
        if (!enemiesToRelease.Contains(ai))
        {
            enemiesToRelease.Enqueue(ai);
        }
    }

    public void RemoveActiveEnemy(EnemyAI ai)
    {
        if (activeEnemies.Contains(ai))
        {
            activeEnemies.Remove(ai);
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector2 playerPos = player.position;

        // 활성 몬스터 로직 순회
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemyAI enemy = activeEnemies[i];

            if (enemy == null) continue;

            if (enemy.isDie) continue;
            enemy.MoveTaget(playerPos);

            if (enemy.isDie) continue;
            enemy.OnUpdate(playerPos);
        }

        // 모든 업데이트 루프가 끝난 직후, 프레임 안전 구역에서 한방에 정리
        CleanUpDeadEnemies();
    }

    private void CleanUpDeadEnemies()
    {
        // 큐에 쌓인 개체들을 하나씩 빼내면서 정리 작업 진행
        while (enemiesToRelease.Count > 0)
        {
            EnemyAI enemy = enemiesToRelease.Dequeue();
            if (enemy == null) continue;
            // 활성 리스트에서 안전하게 제거
            activeEnemies.Remove(enemy);
            if (enemy.usePooling)
            {
                // 잡몹은 해당 ID의 풀로 안전 반환
                if (poolDict.TryGetValue(enemy.GetID(), out var enemyPool))
                {
                    enemyPool.Release(enemy);
                }
            }
            else
            {
                // 풀링을 쓰지 않는 정예몹은 쌩으로 파괴
                Destroy(enemy.gameObject);
            }
        }
    }
    //데미지 누적 코드 
    public void AddDamage(float damageAmount)
    {
        totalDamageDealt += damageAmount;
    }
    //킬 누적 코드
    public void AddKill() { totalKillCount+= 1; }
}