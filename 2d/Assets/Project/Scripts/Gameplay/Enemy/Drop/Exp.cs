using UnityEngine;
using UnityEngine.Pool;

public class Exp : MonoBehaviour
{
    private IObjectPool<Exp> managedPool;
    public int expAmount = 1;

    public void SetPool(IObjectPool<Exp> pool)
    {
        managedPool = pool;
    }

    public void SetExp(int  expAmount)
    {
        this.expAmount = expAmount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어 경험치 증가 로직 호출 
            other.GetComponent<PlayerStats>().TakeExp(expAmount);
            managedPool.Release(this);
        }
    }
}
