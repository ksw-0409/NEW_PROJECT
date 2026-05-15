using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ExpManager : MonoBehaviour
{
    public static ExpManager Instance; // 싱글톤

    public GameObject ExpPrefab; // 경험치 프리팹

    public GameObject player;
    private IObjectPool<Exp> pool;
    // 현재 필드에 떨어져 있는 Exp
    public List<Exp> activeExps = new List<Exp>();
    // 지연 처리를 위한 청소 큐 추가
    private Queue<Exp> expsToRelease = new Queue<Exp>();

    float magnetDistance = 3.0f; // 자석 범위
    float moveSpeed = 10.0f; // 자석 속도

    void Awake()
    {
        Instance = this;

        pool = new ObjectPool<Exp>(
            OnCreateExp,
            OnGetExp,
            OnReleaseExp,
            OnDestroyExp,
            maxSize: 500 // 아이템은 넉넉하게 설정
        );
    }

    private Exp OnCreateExp()
    {
        GameObject obj = Instantiate(ExpPrefab, transform);
        Exp Exp = obj.GetComponent<Exp>();
        Exp.SetPool(pool);
        return Exp;
    }

    private void OnGetExp(Exp exp)
    {
        exp.gameObject.SetActive(true);
        activeExps.Add(exp);
    }

    private void OnReleaseExp(Exp exp)
    {
        exp.gameObject.SetActive(false);
    }

    private void OnDestroyExp(Exp exp)
    {
        Destroy(exp.gameObject);
    }


    // 적이 죽을 때 호출할 함수
    public void DropExp(Vector2 position,float expAmount)
    {
        Exp exp = pool.Get();
        exp.SetExp(expAmount);
        exp.transform.position = position;
    }

    public void EnqueueToRelease(Exp exp)
    {
        if (!expsToRelease.Contains(exp))
        {
            expsToRelease.Enqueue(exp);
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 playerPos = player.transform.position;

        for (int i = 0; i < activeExps.Count; i++)
        {
            Exp exp = activeExps[i];
            if (exp == null || exp.IsEaten) continue; // 이미 먹힌 애는 패스

            float dist = Vector2.Distance(exp.transform.position, playerPos);
            if (dist < magnetDistance)
            {
                exp.transform.position = Vector2.MoveTowards(
                    exp.transform.position,
                    playerPos,
                    moveSpeed * Time.fixedDeltaTime
                );
            }
        }

        CleanUpExps();
    }
    private void CleanUpExps()
    {
        while (expsToRelease.Count > 0)
        {
            Exp exp = expsToRelease.Dequeue();
            if (exp == null) continue;
            activeExps.Remove(exp);
            pool.Release(exp);
        }
    }
}