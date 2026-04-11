using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Pool;

public class ExpManager : MonoBehaviour
{
    public static ExpManager Instance; // 싱글톤

    public GameObject ExpPrefab; // 경험치 프리팹

    public GameObject player;
    private IObjectPool<Exp> pool;
    // 현재 필드에 떨어져 있는 Exp
    public List<Exp> activeExps = new List<Exp>();

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
        activeExps.Remove(exp);
    }

    private void OnDestroyExp(Exp exp)
    {
        Destroy(exp.gameObject);
    }


    // 적이 죽을 때 호출할 함수
    public void DropExp(Vector2 position)
    {
        Exp exp = pool.Get();
        exp.transform.position = position;
    }
    void FixedUpdate()
    {
        if (player == null) return;

        float magnetDistance = 3.0f; // 자석 범위
        float moveSpeed = 10.0f;

        for (int i = activeExps.Count - 1; i >= 0; i--)
        {
            float dist = Vector2.Distance(activeExps[i].transform.position, player.transform.position);

            if (dist < magnetDistance)
            {
                // 플레이어 방향으로 이동
                activeExps[i].transform.position = Vector2.MoveTowards(
                    activeExps[i].transform.position,
                    player.transform.position,
                    moveSpeed * Time.fixedDeltaTime
                );
            }
        }
    }
}