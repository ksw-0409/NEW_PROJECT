using UnityEngine;
using System.Collections;

/// <summary>
/// 화살비와 동일한 방식의 얼음비 장판
/// 차이점: 착지 시 적에게 슬로우 적용
/// </summary>
public class IceRainArea : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fallingIcePrefab;

    [Header("Settings")]
    public float areaRadius = 3f;
    public float shardImpactRadius = 0.35f;

    private float damage;
    private float duration;
    private float slowPercent;
    private float slowDuration;
    private float spawnInterval = 1f;

    private CircleCollider2D areaCollider;
    private Vector3 baseScale;
    private float baseColliderRadius = 1f;

    private void Awake()
    {
        areaCollider = GetComponent<CircleCollider2D>();
        baseScale = transform.localScale;

        if (areaCollider != null)
            baseColliderRadius = Mathf.Max(0.01f, areaCollider.radius);
    }

    public void Setup(
        float dmg,
        float mult,
        float dur,
        float radius,
        float slowPct,
        float slowDur,
        float tickInterval
    )
    {
        damage = dmg * mult;
        duration = dur;
        slowPercent = Mathf.Clamp01(slowPct);
        slowDuration = Mathf.Max(0f, slowDur);
        spawnInterval = Mathf.Max(0.02f, tickInterval);

        areaRadius = Mathf.Max(0.1f, radius * 2f);
        ApplyAreaScale(areaRadius);

        StartCoroutine(RainRoutine());
        Destroy(gameObject, duration + 1f);
    }

    IEnumerator RainRoutine()
    {
        float elapsed = 0f;
        float actualRadius = GetActualWorldRadius();

        while (elapsed < duration)
        {
            Vector2 randomPos2D =
                (Vector2)transform.position + Random.insideUnitCircle * actualRadius;

            Vector3 targetPos = new Vector3(
                randomPos2D.x,
                randomPos2D.y,
                transform.position.z
            );

            Vector3 spawnPos = targetPos + Vector3.up * Random.Range(8f, 12f);

            if (fallingIcePrefab != null)
            {
                GameObject shard = Instantiate(
                    fallingIcePrefab,
                    spawnPos,
                    Quaternion.identity
                );

                shard.transform.localScale = Vector3.one * 0.22f;

                FallingIceShard shardScript = shard.GetComponent<FallingIceShard>();

                if (shardScript != null)
                {
                    shardScript.SetData(
                        damage,
                        shardImpactRadius,
                        targetPos,
                        slowPercent,
                        slowDuration
                    );
                }
            }

            float rainDelay = 0.03f; // 숫자 작을수록 더 많이 내림
            yield return new WaitForSeconds(rainDelay);
            elapsed += rainDelay;
        }
    }

    private void ApplyAreaScale(float targetWorldRadius)
    {
        if (areaCollider == null) return;

        float currentBaseWorldRadius = baseColliderRadius * Mathf.Abs(baseScale.x);
        currentBaseWorldRadius = Mathf.Max(0.01f, currentBaseWorldRadius);

        float scaleFactor = targetWorldRadius / currentBaseWorldRadius;
        transform.localScale = baseScale * scaleFactor;
    }

    private float GetActualWorldRadius()
    {
        if (areaCollider == null)
            return areaRadius;

        return areaCollider.radius * Mathf.Abs(transform.localScale.x);
    }
}

/// <summary>
/// 얼음 조각 낙하
/// </summary>