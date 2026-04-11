using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyAI : MonoBehaviour
{
    public EnemyData data; 
    protected Rigidbody2D rb;
    protected EnemyHealth health; 
    private IObjectPool<EnemyAI> managedPool;
    protected bool isDie = false;

    void Awake()
    {
        rb =GetComponent<Rigidbody2D>();
        health=GetComponent<EnemyHealth>();
    }

    //처음 세팅할때 pool 참조 메니저에서 갖고옴 
    public void SetPool(IObjectPool<EnemyAI> pool)=> managedPool = pool;

    //풀에서 꺼낼때 초기화 함수 Manager에서 호출
    public virtual void Init()
    {
        isDie = false;
        rb.linearVelocity = Vector2.zero; // 이전의 물리 속도 초기화
        if (health != null) health.init(); //체력 초기화
    }

    //죽었을시 비활성화
    private void ReturnToPool()
    {
        if (managedPool != null)
        {
            managedPool.Release(this);
        }
    }

    public virtual void Die()
    {
        if (isDie) return;
        // 여기서 경험치 보석을 생성하거나 이펙트
        isDie = true;
        ExpManager.Instance.DropExp(this.transform.position);
        ItemManager.Instance.DropItem(this.transform.position);
        ReturnToPool();
    }
    public virtual void MoveTaget(Vector2 targetPos)
    {
        if (data == null) return;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;
    }
}
