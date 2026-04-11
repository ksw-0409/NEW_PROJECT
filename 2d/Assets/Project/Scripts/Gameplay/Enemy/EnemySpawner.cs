using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyManager enemyManager; // 아까 만든 매니저 연결
    public Transform player;          // 플레이어 위치 기준

    [Header("설정")]
    public float spawnInterval = 1.0f; // 소환 간격 (초)
    public float spawnDistance = 12.0f; // 플레이어로부터 떨어진 거리

    private float timer;
    void Update()
    {
        if (player == null) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0;
        }
    }

    void Spawn()
    {
        EnemyAI enemy = enemyManager.GetEnemy();
        Vector2 spawnPos = GetRandomPosition();
        enemy.transform.position = spawnPos;
    }

    //앞으로 변경 예정 생성위치 랜덤 로직
    Vector2 GetRandomPosition()
    {
        // 랜덤한 각도(0~360도) 라디안으로 계산
        float angle = Random.Range(0f, Mathf.PI * 2f);

        // 삼각함수로 좌표 구하기: x = cos*r, y = sin*r
        Vector2 spawnOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;

        return (Vector2)player.position + spawnOffset;
    }
}