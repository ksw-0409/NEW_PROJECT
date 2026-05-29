using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;
    public float currentHp; // �ۺ������ �ٲ�

    public float MaxHp => data != null ? data.hp : 100f; // �ִ� ü�� ������ �������� ����

    private IObjectPool<EnemyAI> managedPool;
    public void TakeDamage(float amount)
    {
        GetComponent<EnemyAI>().ApplyHitEffect(EnemyManager.Instance.player.transform.position);
        currentHp -= amount;
        EnemyManager.Instance.AddDamage(amount);
        // ⭐ 전설 어빌리티 1 (그람): 흡혈 — 입힌 피해의 10% 회복
        if (PlayerStats.Instance != null) PlayerStats.Instance.OnDealDamage(amount);
        if (currentHp <= 0)
        {
            // ⭐ 전설 어빌리티 6 (그리브스): 적 처치 시 광폭 스택
            if (PlayerStats.Instance != null) PlayerStats.Instance.OnEnemyKilled();
            GetComponent<EnemyAI>().Die();
        }
    }
    //�ʱ�ȭ ai���� ó��
    public void init(float hp)
    {
        currentHp = hp;
    }
    public void Multiple(float m)
    {
        currentHp = m * currentHp;
    }
}