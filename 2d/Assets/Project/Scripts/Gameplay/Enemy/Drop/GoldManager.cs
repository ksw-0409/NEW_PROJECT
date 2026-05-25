using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance; // 싱글톤

    public GameObject GoldPrefab; // 골드 프리팹

    public GameObject player;
    private IObjectPool<Gold> pool;

    // 현재 필드에 떨어져 있는 Gold
    public List<Gold> activeGolds = new List<Gold>();
    // 지연 처리를 위한 청소 큐 추가
    private Queue<Gold> goldsToRelease = new Queue<Gold>();

    float magnetDistance = 3.0f; // 자석 범위
    float moveSpeed = 10.0f; // 자석 속도

    void Awake()
    {
        Instance = this;

        pool = new ObjectPool<Gold>(
            OnCreateGold,
            OnGetGold,
            OnReleaseGold,
            OnDestroyGold,
            maxSize: 500 // 아이템은 넉넉하게 설정
        );
    }

    private Gold OnCreateGold()
    {
        GameObject obj = Instantiate(GoldPrefab, transform);
        Gold gold = obj.GetComponent<Gold>();
        gold.SetPool(pool);
        return gold;
    }

    private void OnGetGold(Gold gold)
    {
        gold.gameObject.SetActive(true);
        activeGolds.Add(gold);
    }

    private void OnReleaseGold(Gold gold)
    {
        gold.gameObject.SetActive(false);
    }

    private void OnDestroyGold(Gold gold)
    {
        Destroy(gold.gameObject);
    }

    // 적이 죽을 때 호출할 함수
    public void DropGold(Vector2 position, float goldAmount)
    {
        Gold gold = pool.Get();
        gold.SetGold(goldAmount);
        gold.transform.position = position;
    }

    public void EnqueueToRelease(Gold gold)
    {
        if (!goldsToRelease.Contains(gold))
        {
            goldsToRelease.Enqueue(gold);
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 playerPos = player.transform.position;

        for (int i = 0; i < activeGolds.Count; i++)
        {
            Gold gold = activeGolds[i];
            if (gold == null || gold.IsEaten) continue; // 이미 먹힌 애는 패스

            float dist = Vector2.Distance(gold.transform.position, playerPos);
            if (dist < magnetDistance)
            {
                gold.transform.position = Vector2.MoveTowards(
                    gold.transform.position,
                    playerPos,
                    moveSpeed * Time.fixedDeltaTime
                );
            }
        }
        CleanUpGolds();
    }

    private void CleanUpGolds()
    {
        while (goldsToRelease.Count > 0)
        {
            Gold gold = goldsToRelease.Dequeue();
            if (gold == null) continue;
            activeGolds.Remove(gold);
            pool.Release(gold);
        }
    }
}