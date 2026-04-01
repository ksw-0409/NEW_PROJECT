using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyManager : MonoBehaviour
{
    public Transform player; 
    public GameObject enemyPrefab;
    private IObjectPool<EnemyAI> pool;
    private List<EnemyAI> activeEnemies = new List<EnemyAI>();   // 현재 활성화되어 움직여야 할 적들을 따로 관리하는 리스트
    void Awake()
    {
        // 풀 설정
        pool = new ObjectPool<EnemyAI>(
            OnCreateEnemy,           // 생성 시 실행 (Instantiate)
            OnGetEnemy,              // 대여 시 실행 (SetActive true)
            OnReleaseEnemy,          // 반납 시 실행 (SetActive false)
            OnDestroyEnemy,          // 풀 용량 초과 시 실제 파괴
            maxSize: 200             // 최대 보관 개수
        );
    }

    private EnemyAI OnCreateEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, transform);
        EnemyAI ai = obj.GetComponent<EnemyAI>();
        ai.SetPool(pool); // 적에게 풀 참조를 넘겨줘서 스스로 반납하게 함
        return ai;
    }
    private void OnGetEnemy(EnemyAI ai)
    {
        ai.gameObject.SetActive(true);
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

    //필요할 때 적 하나 재활용 or 생성 
    public EnemyAI GetEnemy()
    {
        return pool.Get(); // 이제 루프를 돌지 않고 바로 가져옵니다.
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