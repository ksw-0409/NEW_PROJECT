using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; } // 편의를 위한 싱글톤
    public Transform player;
    private IObjectPool<EnemyAI> pool;
    public List<EnemyAI> activeEnemies = new List<EnemyAI>();   // 현재 활성화되어 움직여야 할 적들을 따로 관리하는 리스트

    private Dictionary<int, IObjectPool<EnemyAI>> poolDict = new Dictionary<int, IObjectPool<EnemyAI>>();

    [Header("다양한 몬스터 프리팹 리스트")]
    public List<EnemyAI> enemyPrefabs;

    void Awake()
    {
        Instance = this; // 싱글톤 초기화
        //딕셔너리 주입 
        foreach (var prefab in enemyPrefabs)
        {
            if (!prefab.usePooling) continue;

            int currentID = prefab.GetID();
            EnemyAI prefabRef = prefab;

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
            if (!poolDict.ContainsKey(currentID))
            {
                poolDict.Add(currentID, pool);
            }
            else
            {
                Debug.LogError($"중복된 몬스터 ID 발견: {prefab.name}. 프리팹 확인이 필요합니다.");
            }
        }
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
        // 프리팹 정보 찾기
        EnemyAI prefab = enemyPrefabs.Find(x => x.GetID() == monsterID);
        if (prefab == null) return null;
        EnemyAI spawnedEnemy = null;
        // 방식에 따른 
        if (prefab.usePooling)
        {
            // 풀링 방식
            spawnedEnemy = poolDict[monsterID].Get();
        }
        else
        {
            // 일반 생성 방식 (정예몹)
            spawnedEnemy = Instantiate(prefab, transform);
            spawnedEnemy.Init();
            activeEnemies.Add(spawnedEnemy); // 리스트에 수동 추가
        }

        spawnedEnemy.transform.position = position;
        return spawnedEnemy;
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
        for(int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null) // Null 체크 추가
            {
                activeEnemies[i].MoveTaget(playerPos);
            }
        }
    }
}