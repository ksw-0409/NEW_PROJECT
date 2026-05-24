using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;
    public float currentHp; // 퍼블릭으로 바꿈

    public float MaxHp => data != null ? data.hp : 100f; // 최대 체력 데이터 가저오는 변수

    private IObjectPool<EnemyAI> managedPool;
    public void TakeDamage(float amount)
    {
        GetComponent<EnemyAI>().ApplyHitEffect(EnemyManager.Instance.player.transform.position);
        currentHp -= amount;
        EnemyManager.Instance.AddDamage(amount);
        if (currentHp <= 0)
        {
            GetComponent<EnemyAI>().Die();
        }
    }
    //초기화 ai에서 처리
    public void init(float hp)
    {
        currentHp = hp;
    }
    public void Multiple(float m)
    {
        currentHp = m * currentHp;
    }
}