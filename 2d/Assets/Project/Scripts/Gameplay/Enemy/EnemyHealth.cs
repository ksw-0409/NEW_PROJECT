using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;
    private float currentHp;
    private IObjectPool<EnemyAI> managedPool;
    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            Die();
        }
    }
    //OnEnable 활성화될때마다 자동 호출
    void OnEnable()
    {
        currentHp = data.hp;
    }

    void Die()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai == null) return;

        // 여기서 경험치 보석을 생성하거나 이펙트
        GetComponent<EnemyAI>().ReturnToPool();
        ItemManager.Instance.DropItem(this.transform.position);
    }
}