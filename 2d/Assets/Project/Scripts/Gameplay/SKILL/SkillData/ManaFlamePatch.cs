using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 푸른 마나 불꽃 패치 — 영역 내 적들에게 일정 주기로 지속 피해.
/// SwordWaveProjectile의 firezone 특수효과가 스폰합니다.
/// </summary>
public class ManaFlamePatch : MonoBehaviour
{
    [Tooltip("패치 지속 시간 (초)")]
    public float duration = 2f;

    [Tooltip("초당 데미지")]
    public float dps = 0f;

    [Tooltip("데미지 틱 간격 (초)")]
    public float tickInterval = 0.5f;

    private float lifeTimer;
    private float tickTimer;

    // 같은 적이 짧은 시간 안에 여러 패치에 들어와도 한 번씩만 맞도록 추적
    private readonly Dictionary<EnemyHealth, float> lastDamagedAt = new Dictionary<EnemyHealth, float>();

    void Update()
    {
        lifeTimer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (lifeTimer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // 틱 주기에 도달하면 영역 내 모든 적에게 데미지
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ApplyTick();
        }
    }

    void ApplyTick()
    {
        var col = GetComponent<CircleCollider2D>();
        if (col == null) return;

        float radius = col.radius * Mathf.Abs(transform.lossyScale.x);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        // 틱당 데미지 = dps * tickInterval
        float tickDamage = dps * tickInterval;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) continue;
            enemy.TakeDamage(tickDamage);
        }
    }
}
