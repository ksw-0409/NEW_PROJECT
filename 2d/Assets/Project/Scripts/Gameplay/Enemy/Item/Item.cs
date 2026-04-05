using UnityEngine;
using UnityEngine.Pool;

public class Item : MonoBehaviour
{
    private IObjectPool<Item> managedPool;
    public int expAmount = 1;

    public void SetPool(IObjectPool<Item> pool)
    {
        managedPool = pool;
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
