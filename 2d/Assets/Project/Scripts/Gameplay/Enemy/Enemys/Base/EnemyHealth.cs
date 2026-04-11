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
            GetComponent<EnemyAI>().Die();
        }
    }
    //초기화 ai에서 처리
    public void init()
    {
        currentHp = data.hp;
    }
}