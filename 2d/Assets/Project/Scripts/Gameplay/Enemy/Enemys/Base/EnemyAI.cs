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
    private float stunTimer = 0f;
    private float slowTimer = 0f;
    private float slowMultiplier = 1f;
    public bool IsStunned => stunTimer > 0f;

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
        stunTimer = 0f;
        slowTimer = 0f;
        slowMultiplier = 1f;
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
    public virtual void MoveTaget(Vector2 targetPos) // 슬로우 적용되게 바꿈
    {
        if (data == null) return;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;

        float currentSpeed = data.moveSpeed;

        if (slowTimer > 0f)
            currentSpeed *= slowMultiplier;

        rb.linearVelocity = dir * currentSpeed;
    }

    public void TickCrowdControl(float deltaTime)
    {
        if (stunTimer > 0f)
        {
            stunTimer -= deltaTime;
            if (stunTimer < 0f) stunTimer = 0f;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (slowTimer > 0f)
        {
            slowTimer -= deltaTime;
            if (slowTimer < 0f)
            {
                slowTimer = 0f;
                slowMultiplier = 1f;
            }
            rb.linearVelocity *= slowMultiplier;
        }
    }
    private void Update()
    {
        TickCrowdControl(Time.deltaTime); // TickCrowdControl을 Update에서 호출하여 매 프레임마다 상태를 업데이트
    }

    public void ApplyKnockbackAndStun(Vector2 sourcePos, float knockbackForce, float stunDuration)
    {
        if (rb == null) return;
        Vector2 dir = ((Vector2)transform.position - sourcePos).normalized;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        stunTimer = Mathf.Max(stunTimer, stunDuration);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);
    }
}
