using UnityEngine;
using UnityEngine.Pool;

public class Gold : MonoBehaviour
{
    private IObjectPool<Gold> managedPool;
    public bool IsEaten { get; private set; } = false;
    public float goldAmount = 1;


    public void SetPool(IObjectPool<Gold> pool)
    {
        managedPool = pool;
        IsEaten = false; // 풀에서 꺼낼 때 초기화
    }

    public void SetGold(float amount)
    {
        this.goldAmount = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsEaten = true;
            GameDataManager.Instance.AddGold(Mathf.FloorToInt(goldAmount));
            GoldManager.Instance.EnqueueToRelease(this);
        }
    }
}