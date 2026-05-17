using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 내려찍기 지진 균열 지대 — 적이 접촉하면 둔화 적용
/// </summary>
public class QuakeZone : MonoBehaviour
{
    public float duration = 3f;
    public float slowPercent = 0.4f;
    public float slowDuration = 3f;

    private float elapsed;
    private float tickTimer;
    private const float TickInterval = 0.5f;

    void Update()
    {
        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;
        if (elapsed >= duration) return;

        if (tickTimer >= TickInterval)
        {
            tickTimer = 0f;
            ApplyTick();
        }
    }

    void ApplyTick()
    {
        var col = GetComponent<CircleCollider2D>();
        if (col == null) return;
        float r = col.radius * Mathf.Abs(transform.lossyScale.x);
        var hits = Physics2D.OverlapCircleAll(transform.position, r);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            var slow = h.GetComponent<EnemySlow>();
            if (slow != null) slow.ApplySlow(slowPercent, slowDuration);
        }
    }
}
