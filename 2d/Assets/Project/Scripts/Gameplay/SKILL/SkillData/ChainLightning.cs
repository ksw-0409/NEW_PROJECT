using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainLightning : MonoBehaviour
{
    [Header("Visual Settings")]
    public GameObject hitEffectPrefab; // ⭐ 적 위치에 생성할 번개 이미지 프리팹
    private LineRenderer lineRenderer;

    private float damage;
    private int maxChainCount;
    private float chainRadius;
    private List<Transform> hitEnemies = new List<Transform>();
    private Transform playerTransform;
    private bool overloadEnabled;
    private float overloadPerJump = 0.2f;
    private bool finalExplosionEnabled;
    private float finalExplosionRadius = 3.5f;
    private float finalExplosionMultiplier = 1.8f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // 시작 시 선이 안 보이게 초기화
        lineRenderer.positionCount = 0;
    }

    public void Setup(float baseDamage, int chains, float range, Transform player)
    {
        this.damage = baseDamage;
        this.maxChainCount = chains;
        this.chainRadius = range;
        this.playerTransform = player;
        overloadEnabled = PlayerStats.Instance != null &&
                          PlayerStats.Instance.HasAnySpecialty("Lightning_Overload", "Special_LightningOverload", "ChainLightning_Overload");
        finalExplosionEnabled = PlayerStats.Instance != null &&
                                PlayerStats.Instance.HasAnySpecialty("Lightning_FinalExplosion", "Lightning_BigExplosion", "Special_LightningFinal", "Lightning_Explosion");
    }

    public void StartChain(Transform firstTarget)
    {
        StartCoroutine(ChainRoutine(firstTarget));
    }

    IEnumerator ChainRoutine(Transform target)
    {
        int currentChain = 0;
        Transform previousPoint = playerTransform; // 시작은 플레이어
        Transform lastHitTarget = null;

        while (target != null && currentChain < maxChainCount)
        {
            // 1. 데미지 입히기
            EnemyHealth health = target.GetComponent<EnemyHealth>();
            if (health != null)
            {
                float hitDamage = damage;
                if (overloadEnabled)
                    hitDamage *= 1f + (overloadPerJump * currentChain);

                health.TakeDamage(hitDamage);
            }
            hitEnemies.Add(target);
            lastHitTarget = target;

            // 2. ⭐ 적 위치에 번개 이미지 이펙트 생성
            if (hitEffectPrefab != null)
            {
                // 적의 위치에 이펙트를 만들고 0.3초 뒤에 자동으로 사라지게 함
                GameObject effect = Instantiate(hitEffectPrefab, target.position, Quaternion.identity);
                Destroy(effect, 0.3f);
            }

            // 3. 선 그리기 (이전 포인트와 현재 타겟 연결)
            // 코루틴 안에서 또 코루틴을 기다림 (선이 유지되는 동안 멈춤)
            yield return StartCoroutine(DrawLineUpdate(previousPoint, target));

            // 4. 다음 타겟 찾기
            previousPoint = target;
            target = FindNextTarget(target);
            currentChain++;
        }

        if (finalExplosionEnabled && lastHitTarget != null)
        {
            TriggerFinalExplosion(lastHitTarget.position);
        }

        // 모든 연쇄가 끝나면 선 지우고 이 스크립트가 붙은 오브젝트 삭제
        lineRenderer.positionCount = 0;
        Destroy(gameObject);
    }

    // 두 지점 사이를 실시간으로 연결하는 로직
    IEnumerator DrawLineUpdate(Transform start, Transform end)
    {
        float elapsed = 0;
        float duration = 0.15f;
        int segments = 5; // 선을 5개 구간으로 나눔
        lineRenderer.positionCount = segments + 1;

        while (elapsed < duration)
        {
            if (start != null && end != null)
            {
                lineRenderer.SetPosition(0, start.position);
                for (int i = 1; i < segments; i++)
                {
                    Vector3 pos = Vector3.Lerp(start.position, end.position, (float)i / segments);
                    // 찌릿찌릿한 느낌을 위해 랜덤 오차 추가
                    pos += (Vector3)Random.insideUnitCircle * 0.3f;
                    lineRenderer.SetPosition(i, pos);
                }
                lineRenderer.SetPosition(segments, end.position);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private Transform FindNextTarget(Transform currentTarget)
    {
        // 원형 범위 안의 모든 충돌체 감지
        Collider2D[] nextEnemies = Physics2D.OverlapCircleAll(currentTarget.position, chainRadius);
        Transform bestTarget = null;
        float closeDist = Mathf.Infinity;

        foreach (var col in nextEnemies)
        {
            if (!col.CompareTag("Enemy"))
                continue;

            EnemyHealth enemyHealth = col.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null || !enemyHealth.gameObject.activeInHierarchy)
                continue;

            Transform enemyRoot = enemyHealth.transform;
            // 현재 타겟 기준으로 가장 가까운 "아직 맞지 않은" 적을 연쇄
            if (!hitEnemies.Contains(enemyRoot))
            {
                float dist = Vector2.Distance(currentTarget.position, enemyRoot.position);
                if (dist < closeDist)
                {
                    closeDist = dist;
                    bestTarget = enemyRoot;
                }
            }
        }
        return bestTarget;
    }

    private void TriggerFinalExplosion(Vector2 center)
    {
        SpawnBlueRingEffect(center, finalExplosionRadius, 0.35f);
        SpawnBlueFlash(center, finalExplosionRadius * 1.6f, 0.2f);

        if (hitEffectPrefab != null)
        {
            GameObject fx = Instantiate(hitEffectPrefab, center, Quaternion.identity);
            fx.transform.localScale *= 2.5f;
            Destroy(fx, 0.5f);
        }

        Collider2D[] targets = Physics2D.OverlapCircleAll(center, finalExplosionRadius);
        for (int i = 0; i < targets.Length; i++)
        {
            if (!targets[i].CompareTag("Enemy"))
                continue;

            EnemyHealth enemy = targets[i].GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage * finalExplosionMultiplier);
            }
        }
    }

    private void SpawnBlueRingEffect(Vector2 center, float radius, float lifetime)
    {
        GameObject ring = new GameObject("LightningFinalExplosionRing");
        ring.transform.position = center;

        LineRenderer lr = ring.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 48;
        lr.startWidth = 0.12f;
        lr.endWidth = 0.12f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.25f, 0.7f, 1f, 0.95f);
        lr.endColor = new Color(0.25f, 0.7f, 1f, 0.95f);
        lr.sortingOrder = 220;

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = (float)i / lr.positionCount;
            float angle = t * Mathf.PI * 2f;
            Vector3 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            lr.SetPosition(i, p);
        }

        Destroy(ring, lifetime);
    }

    private void SpawnBlueFlash(Vector2 center, float scale, float lifetime)
    {
        GameObject flash = new GameObject("LightningFinalExplosionFlash");
        flash.transform.position = center;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhitePixelSprite();
        sr.color = new Color(0.4f, 0.85f, 1f, 0.45f);
        sr.sortingOrder = 210;
        flash.transform.localScale = new Vector3(scale, scale, 1f);
        Destroy(flash, lifetime);
    }

    private static Sprite cachedWhitePixel;
    private Sprite CreateWhitePixelSprite()
    {
        if (cachedWhitePixel != null) return cachedWhitePixel;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        cachedWhitePixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedWhitePixel;
    }
}