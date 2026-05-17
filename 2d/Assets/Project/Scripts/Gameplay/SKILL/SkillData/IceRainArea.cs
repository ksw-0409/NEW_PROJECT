using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 화살비와 동일한 방식의 얼음비 장판
/// 특수효과:
///   IceRain_burst  : 둔화/빙결된 적이 죽을 때 주변에 얼음 파편 폭발
///   IceRain_frozen : 영역에서 2초 이상 버틴 적 1.5초간 완전 빙결
///   IceRain_spear  : 비 대신 거대 얼음 창 낙하 (직격 300%)
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

    // ✨ 영구 동토: 영역 안 적의 체류 시간 추적
    private Dictionary<EnemyHealth, float> enemyTimeInside = new Dictionary<EnemyHealth, float>();
    private HashSet<EnemyHealth> alreadyFrozen = new HashSet<EnemyHealth>();

    // 특수효과 플래그 (캐시)
    private bool hasBurst;
    private bool hasFrozen;
    private bool hasSpear;

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

        // 특수효과 캐시
        if (PlayerStats.Instance != null)
        {
            hasBurst  = PlayerStats.Instance.HasSpecialty("IceRain_burst");
            hasFrozen = PlayerStats.Instance.HasSpecialty("IceRain_frozen");
            hasSpear  = PlayerStats.Instance.HasSpecialty("IceRain_spear");
        }

        // 얼음비 장판 시작 시 흔들림
        CameraShake.ShakePreset(CameraShake.Preset.Light);

        StartCoroutine(RainRoutine());
        if (hasFrozen) StartCoroutine(FrozenTracker());

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

            Vector3 targetPos = new Vector3(randomPos2D.x, randomPos2D.y, transform.position.z);
            Vector3 spawnPos = targetPos + Vector3.up * Random.Range(8f, 12f);

            if (fallingIcePrefab != null)
            {
                GameObject shard = Instantiate(fallingIcePrefab, spawnPos, Quaternion.identity);

                // ✨ [변칙] 고드름 낙하: 큰 얼음 창
                float scaleMult = hasSpear ? 0.45f : 0.22f;       // 더 큰 크기
                float dmgMult   = hasSpear ? 3.0f  : 1.0f;        // 직격 300%
                shard.transform.localScale = Vector3.one * scaleMult;

                if (hasSpear)
                {
                    var sr = shard.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.7f, 0.95f, 1f, 1f); // 더 밝은 청색
                }

                FallingIceShard shardScript = shard.GetComponent<FallingIceShard>();

                if (shardScript != null)
                {
                    shardScript.SetData(
                        damage * dmgMult,
                        shardImpactRadius * (hasSpear ? 1.2f : 1.0f),
                        targetPos,
                        slowPercent,
                        slowDuration,
                        hasBurst   // burst 효과는 shard에 전달해서 적 사망 시 폭발하게
                    );
                }
            }

            // 고드름 모드는 발사 간격이 더 김 (희소함)
            float rainDelay = hasSpear ? 0.18f : 0.03f;
            yield return new WaitForSeconds(rainDelay);
            elapsed += rainDelay;
        }
    }

    // ✨ [유틸] 영구 동토: 영역 내 적의 체류 시간 추적, 2초 넘으면 1.5초 빙결
    IEnumerator FrozenTracker()
    {
        const float frozenThreshold = 2f;
        const float frozenDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(0.25f);
            elapsed += 0.25f;

            float r = GetActualWorldRadius();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, r);

            // 현재 영역 안의 적 목록
            var currentEnemies = new HashSet<EnemyHealth>();
            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                var e = h.GetComponent<EnemyHealth>();
                if (e == null) continue;
                currentEnemies.Add(e);

                if (alreadyFrozen.Contains(e)) continue;

                if (!enemyTimeInside.ContainsKey(e))
                    enemyTimeInside[e] = 0f;
                enemyTimeInside[e] += 0.25f;

                if (enemyTimeInside[e] >= frozenThreshold)
                {
                    // 빙결 적용
                    StartCoroutine(FreezeEnemy(e, frozenDuration));
                    alreadyFrozen.Add(e);
                }
            }

            // 영역 밖으로 나간 적은 카운터 리셋
            var toRemove = new List<EnemyHealth>();
            foreach (var kv in enemyTimeInside)
                if (!currentEnemies.Contains(kv.Key)) toRemove.Add(kv.Key);
            foreach (var e in toRemove) enemyTimeInside.Remove(e);
        }
    }

    IEnumerator FreezeEnemy(EnemyHealth enemy, float dur)
    {
        if (enemy == null) yield break;
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai == null) yield break;

        // EnemyAI 비활성화로 빙결 (이동/공격 정지)
        ai.enabled = false;

        // 시각: 청백색 틴트
        var sr = enemy.GetComponentInChildren<SpriteRenderer>();
        Color original = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = new Color(0.5f, 0.8f, 1f, 1f);

        yield return new WaitForSeconds(dur);

        if (ai != null) ai.enabled = true;
        if (sr != null) sr.color = original;

        // 빙결 해제 후 다시 빙결될 수 있도록 카운터 리셋
        if (enemy != null)
        {
            alreadyFrozen.Remove(enemy);
            enemyTimeInside.Remove(enemy);
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
        if (areaCollider == null) return areaRadius;
        return areaCollider.radius * Mathf.Abs(transform.localScale.x);
    }
}
