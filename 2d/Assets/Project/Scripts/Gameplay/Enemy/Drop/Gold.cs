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
        IsEaten = false; // Ǯ���� ���� �� �ʱ�ȭ
    }

    public void SetGold(float amount)
    {
        this.goldAmount = amount;
        IsEaten = false; // Ǯ���� ���� �� �ʱ�ȭ
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsEaten = true;
            // 골드 획득 패시브 적용: passive_gold 레벨에 따라 획득량 증가
            float finalGold = goldAmount;
            if (PassiveSystem.Instance != null)
                finalGold *= PassiveSystem.Instance.GetMultiplier(PassiveSystem.ID_GOLD);
            GameDataManager.Instance.AddGold(Mathf.FloorToInt(finalGold));
            GoldManager.Instance.EnqueueToRelease(this);
        }
    }
}