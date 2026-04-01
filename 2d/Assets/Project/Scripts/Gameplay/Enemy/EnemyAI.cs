using UnityEngine;
using UnityEngine.Pool;

public class EnemyAI : MonoBehaviour
{
    public EnemyData data;
    private Rigidbody2D rb;

    // 이 적을 관리하는 풀에 대한 참조
    private IObjectPool<EnemyAI> managedPool;

    void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
    }
    public void SetPool(IObjectPool<EnemyAI> pool)
    {
        managedPool = pool;
    }

    public virtual void MoveTaget(Vector2 targetPos)
    {
        if (data == null) return;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;
    }
}
