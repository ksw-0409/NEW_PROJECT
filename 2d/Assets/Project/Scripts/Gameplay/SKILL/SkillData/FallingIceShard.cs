using UnityEngine;

public class FallingIceShard : MonoBehaviour
{
    private float damage;
    private float radius;
    private Vector3 targetPos;
    private bool hasImpacted = false;

    private float slowPercent;
    private float slowDuration;

    public float fallSpeed = 25f;

    public void SetData(
        float dmg,
        float rad,
        Vector3 target,
        float slowPct,
        float slowDur
    )
    {
        damage = dmg;
        radius = rad;
        targetPos = target;
        slowPercent = slowPct;
        slowDuration = slowDur;
        hasImpacted = false;

        transform.right = Vector3.down;
    }

    void Update()
    {
        if (hasImpacted) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            fallSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            PerformImpact();
        }
    }

    private void PerformImpact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var target in targets)
        {
            if (!target.CompareTag("Enemy")) continue;

            EnemyHealth health =
                target.GetComponentInParent<EnemyHealth>() ??
                target.GetComponent<EnemyHealth>();

            if (health != null)
                health.TakeDamage(damage);

            EnemyAI ai =
                target.GetComponentInParent<EnemyAI>() ??
                target.GetComponent<EnemyAI>();

            if (ai != null)
            {
                float speedMul = Mathf.Clamp(1f - slowPercent, 0.1f, 1f);
                ai.ApplySlow(speedMul, slowDuration);
            }
        }

        Destroy(gameObject);
    }
}