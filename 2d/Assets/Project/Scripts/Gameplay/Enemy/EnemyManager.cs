using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public Transform player;
    public List<EnemyAI> enemyPool = new List<EnemyAI>(); 
    public int poolSize = 100;    // 미리 만들어둘 개수
    public GameObject enemyPrefab;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewEnemy();
        }
    }
    private EnemyAI CreateNewEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, transform);
        obj.SetActive(false);
        EnemyAI ai = obj.GetComponent<EnemyAI>();
        if (ai != null) enemyPool.Add(ai);
        return ai;
    }

    //필요할 때 적 하나 재활용 or 생성 
    public EnemyAI GetEnemy()
    {
        foreach (EnemyAI ai in enemyPool)
        {
            if (!ai.gameObject.activeInHierarchy) return ai;
        }
        return CreateNewEnemy(); // 모자라면 더 생성
    }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector2 playerPos = player.position;
        // 모든 적에게 이동 명령
        for (int i= 0; i<enemyPool.Count;i++)
        {
            // SetActive 상태인 적만 이동 계산을 수행
            if (enemyPool[i].gameObject.activeInHierarchy)
            {
                enemyPool[i].MoveTaget(playerPos);
            }
        }
    }

}