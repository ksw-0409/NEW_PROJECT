using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;
    public float currentHp;

    public float MaxHp => data != null ? data.hp : 100f;

    private IObjectPool<EnemyAI> managedPool;

    /// <summary>
    /// 일반 데미지 (자동 critical roll). 다른 시스템에서 critical을 이미 계산했으면 isCriticalAlreadyApplied=true.
    /// </summary>
    public void TakeDamage(float amount, bool isCriticalAlreadyApplied = false)
    {
        bool isCrit = false;

        // ⭐ critical roll (이미 사전 계산된 게 아니면)
        if (!isCriticalAlreadyApplied && PlayerStats.Instance != null)
        {
            float critChance = PlayerStats.Instance.CriticalChance;
            if (Random.value < critChance)
            {
                isCrit = true;
                float critMul = PlayerStats.Instance.CriticalDamage;
                if (critMul < 1.01f) critMul = 1.5f; // 안전장치
                amount *= critMul;
            }
        }
        else
        {
            // 이미 사전 계산됨 — isCriticalAlreadyApplied 매개변수가 곧 isCrit 플래그
            isCrit = isCriticalAlreadyApplied;
        }

        GetComponent<EnemyAI>().ApplyHitEffect(EnemyManager.Instance.player.transform.position);
        currentHp -= amount;
        EnemyManager.Instance.AddDamage(amount);

        // ⭐ 전설 어빌리티 1 (그람): 흡혈
        if (PlayerStats.Instance != null) PlayerStats.Instance.OnDealDamage(amount);

        // ⭐ 데미지 숫자 표시 (적 머리 살짝 위)
        FloatingDamageText.Spawn(transform.position + new Vector3(0f, 0.5f, 0f), amount, isCrit);

        if (currentHp <= 0)
        {
            // ⭐ 전설 어빌리티 6 (그리브스)
            if (PlayerStats.Instance != null) PlayerStats.Instance.OnEnemyKilled();
            GetComponent<EnemyAI>().Die();
        }
    }

    public void init(float hp)
    {
        currentHp = hp;
    }
    public void Multiple(float m)
    {
        currentHp = m * currentHp;
    }
}
