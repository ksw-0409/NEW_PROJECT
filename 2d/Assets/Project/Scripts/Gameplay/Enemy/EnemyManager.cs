using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyManager : MonoBehaviour
{
    public Transform player;
    private IObjectPool<EnemyAI> pool;
    public List<EnemyAI> activeEnemies = new List<EnemyAI>();   // 현재 활성화되어 움직여야 할 적들을 따로 관리하는 리스트

    private Dictionary<string, IObjectPool<EnemyAI>> poolDict = new Dictionary<string, IObjectPool<EnemyAI>>();

    [Header("다양한 몬스터 프리팹 리스트")]
    public List<EnemyAI> enemyPrefabs;

    void Awake()
    {
        //딕셔너리 주입 
        foreach (var prefab in enemyPrefabs)
        {
            EnemyAI prefabRef = prefab;

            var pool = new ObjectPool<EnemyAI>(
                createFunc: () => {
                    EnemyAI ai = Instantiate(prefabRef, transform);
                    ai.SetPool(poolDict[prefabRef.name]); // 각자의 풀 참조 주입
                    return ai;
                },
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                actionOnDestroy: OnDestroyEnemy,
                maxSize: 100
            );

            poolDict.Add(prefabRef.name, pool);
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

    public EnemyAI GetEnemy(string monsterName)
    {
        // 딕셔너리에서 이름으로 풀을 찾아서 Get
        if (poolDict.TryGetValue(monsterName, out var pool))
        {
            return pool.Get();
        }
        Debug.LogError($"{monsterName} 풀이 존재하지 않습니다!");
        return null;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector2 playerPos = player.position;
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            activeEnemies[i].MoveTaget(playerPos);
        }
    }
}