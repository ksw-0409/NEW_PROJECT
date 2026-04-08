using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyAI : MonoBehaviour
{
    public EnemyData data;
    private Rigidbody2D rb;

    // 이 적을 관리하는 풀
    private IObjectPool<EnemyAI> managedPool;

    void Awake()
    {
        rb =GetComponent<Rigidbody2D>();
    }

    //처음 세팅할때 pool 참조 메니저에서 갖고옴 
    public void SetPool(IObjectPool<EnemyAI> pool)
    {
        managedPool = pool;
    }
    
    //죽었을시 비활성화
    public void ReturnToPool()
    {
        if (managedPool != null)
        {
            managedPool.Release(this);
        }
    }

    public virtual void MoveTaget(Vector2 targetPos)
    {
        if (data == null) return;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;
    }
}
