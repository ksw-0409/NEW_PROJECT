using UnityEngine;
using System.Collections.Generic;

public class FireField : MonoBehaviour
{
    private float damagePerSecond;
    private float tickInterval = 0.5f; // 0.5초마다 데미지 틱
    private float slowMultiplier = 1f;
    private float slowDuration = 0f;
    private readonly Dictionary<EnemyHealth, float> enemyTickTimers = new Dictionary<EnemyHealth, float>();

    public void Setup(float duration, float damage, float slowMul = 1f, float slowDur = 0f)
    {
        this.damagePerSecond = damage;
        this.slowMultiplier = Mathf.Clamp(slowMul, 0.1f, 1f);
        this.slowDuration = Mathf.Max(0f, slowDur);
        // 지정된 시간 후 장판 삭제
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth health = collision.GetComponentInParent<EnemyHealth>();
            if (health == null) return;

            if (!enemyTickTimers.ContainsKey(health))
                enemyTickTimers[health] = 0f;

            enemyTickTimers[health] += Time.deltaTime;
            if (enemyTickTimers[health] >= tickInterval)
            {
                enemyTickTimers[health] = 0f;
                // 0.5초마다 초당 데미지의 절반씩 입힘
                if (health != null) health.TakeDamage(damagePerSecond * tickInterval);
            }

            if (slowMultiplier < 0.999f && slowDuration > 0f)
            {
                EnemyAI ai = collision.GetComponentInParent<EnemyAI>();
              //  if (ai != null) ai.ApplySlow(slowMultiplier, slowDuration);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EnemyHealth health = collision.GetComponentInParent<EnemyHealth>();
        if (health != null && enemyTickTimers.ContainsKey(health))
            enemyTickTimers.Remove(health);
    }
}