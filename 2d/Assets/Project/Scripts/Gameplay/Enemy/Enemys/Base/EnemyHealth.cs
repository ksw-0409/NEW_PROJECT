using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] protected EnemyData data;
    private float currentHp;
    private IObjectPool<EnemyAI> managedPool;
    private float Defense;
    public void TakeDamage(float amount)
    {
        float d = (amount * 100) / (100 + Defense);
        currentHp -= Mathf.Round(d);
        Debug.Log(currentHp);
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
}