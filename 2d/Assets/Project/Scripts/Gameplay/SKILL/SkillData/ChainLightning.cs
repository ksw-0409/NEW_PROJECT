using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainLightning : MonoBehaviour
{
    [Header("Visual Settings")]
    public GameObject hitEffectPrefab; // 적 위치에 생성할 번개 폭발 이펙트
    private LineRenderer lineRenderer;

    [Header("Lightning Texture (lightning_2_blue 6 frames)")]
    [Tooltip("번개 라인에 입힐 텍스처 6장. 비어있으면 자동 로드.")]
    public Sprite[] lightningFrames;

    private Material lineMaterial;
    private float frameTimer = 0f;
    private int currentFrame = 0;

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
        lineRenderer.positionCount = 0;

        // 자동 로드: lightning_2_blue_1 ~ _6
        if (lightningFrames == null || lightningFrames.Length == 0)
        {
            lightningFrames = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                string path = "Assets/sanctum_pixel/lightning_2_package/Sprite/lightning_2/lightning_2_blue/lightning_2_blue_" + (i + 1) + ".png";
#if UNITY_EDITOR
                lightningFrames[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#endif
            }
        }

        // LineRenderer 설정: 더 굵게, 텍스처 적용
        lineRenderer.startWidth = 0.30f;
        lineRenderer.endWidth = 0.30f;
        lineRenderer.numCapVertices = 2;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.alignment = LineAlignment.TransformZ; // ⭐ 빌드/에디터 두께 일관 (View는 카메라 의존)
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingOrder = 200;

        // 머티리얼: 첫 프레임 텍스처를 입혀서 굵은 번개 느낌
        if (lightningFrames != null && lightningFrames.Length > 0 && lightningFrames[0] != null)
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.mainTexture = lightningFrames[0].texture;
            lineRenderer.material = lineMaterial;
        }
        else
        {
            // sprite 못 찾으면 fallback: 청백색 색상만
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0.4f, 0.85f, 1f, 1f);
            lineRenderer.endColor = new Color(0.8f, 0.95f, 1f, 1f);
        }
    }

    void Update()
    {
        // 6프레임 애니메이션 — 약 0.06초 간격으로 텍스처 교체
        if (lineMaterial == null || lightningFrames == null || lightningFrames.Length == 0) return;
        if (lineRenderer.positionCount < 2) return; // 그리는 중이 아니면 skip

        frameTimer += Time.deltaTime;
        if (frameTimer >= 0.06f)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % lightningFrames.Length;
            if (lightningFrames[currentFrame] != null)
                lineMaterial.mainTexture = lightningFrames[currentFrame].texture;
        }
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
        Transform previousPoint = playerTransform;
        Transform lastHitTarget = null;

        while (target != null && currentChain < maxChainCount)
        {
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

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, target.position, Quaternion.identity);
                Destroy(effect, 0.3f);
            }

            yield return StartCoroutine(DrawLineUpdate(previousPoint, target));

            previousPoint = target;
            target = FindNextTarget(target);
            currentChain++;
        }

        if (finalExplosionEnabled && lastHitTarget != null)
        {
            TriggerFinalExplosion(lastHitTarget.position);
        }

        lineRenderer.positionCount = 0;
        Destroy(gameObject);
    }

    IEnumerator DrawLineUpdate(Transform start, Transform end)
    {
        float duration = 0.22f;
        int segments = 10;
        lineRenderer.positionCount = segments + 1;

        // ✨ sprite 기반 추가 비주얼 — 두 점 사이에 lightning_2_blue sprite를 stretch
        GameObject spriteGo = null;
        SpriteRenderer spriteSr = null;
        if (lightningFrames != null && lightningFrames.Length > 0 && start != null && end != null)
        {
            spriteGo = new GameObject("LightningBetween");
            spriteSr = spriteGo.AddComponent<SpriteRenderer>();
            spriteSr.sprite = lightningFrames[0];
            spriteSr.sortingOrder = 199;
        }

        float elapsed = 0;
        int spriteFrame = 0;
        float spriteFrameTimer = 0f;

        while (elapsed < duration)
        {
            if (start != null && end != null)
            {
                // 1) LineRenderer 굵은 지그재그 번개
                lineRenderer.SetPosition(0, start.position);
                for (int i = 1; i < segments; i++)
                {
                    Vector3 pos = Vector3.Lerp(start.position, end.position, (float)i / segments);
                    pos += (Vector3)Random.insideUnitCircle * 0.6f;
                    lineRenderer.SetPosition(i, pos);
                }
                lineRenderer.SetPosition(segments, end.position);

                // 2) sprite를 두 점 사이에 stretch
                if (spriteGo != null && spriteSr != null)
                {
                    Vector3 mid = (start.position + end.position) * 0.5f;
                    Vector3 dir = end.position - start.position;
                    float length = dir.magnitude;

                    spriteGo.transform.position = mid;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    spriteGo.transform.rotation = Quaternion.Euler(0, 0, angle);

                    // sprite 원본 너비를 length에 맞게 scale 조정
                    // lightning_2_blue는 96x96 + PPU=32, native size 3 units
                    // 두께는 0.7 정도 (적당히 굵게)
                    spriteGo.transform.localScale = new Vector3(length / 3f, 0.7f, 1f);

                    // sprite 프레임 애니메이션
                    spriteFrameTimer += Time.deltaTime;
                    if (spriteFrameTimer >= 0.05f)
                    {
                        spriteFrameTimer = 0f;
                        spriteFrame = (spriteFrame + 1) % lightningFrames.Length;
                        if (lightningFrames[spriteFrame] != null) spriteSr.sprite = lightningFrames[spriteFrame];
                    }
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spriteGo != null) Destroy(spriteGo);
    }

    private Transform FindNextTarget(Transform currentTarget)
    {
        Collider2D[] nextEnemies = Physics2D.OverlapCircleAll(currentTarget.position, chainRadius);
        Transform bestTarget = null;
        float closeDist = Mathf.Infinity;

        foreach (var col in nextEnemies)
        {
            if (!col.CompareTag("Enemy")) continue;
            EnemyHealth enemyHealth = col.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null || !enemyHealth.gameObject.activeInHierarchy) continue;

            Transform enemyRoot = enemyHealth.transform;
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
        // ⭐ 동그라미 번개 폭발 이펙트 (사용자 요청 — 방사형 번개 + 충격파 링 + 코어 글로우)
        LightningCircleExplosion.Spawn(center, finalExplosionRadius, 0.55f);

        // 데미지 영역 (시각 효과와 별개 — 변경 없음)
        Collider2D[] targets = Physics2D.OverlapCircleAll(center, finalExplosionRadius);
        for (int i = 0; i < targets.Length; i++)
        {
            if (!targets[i].CompareTag("Enemy")) continue;
            EnemyHealth enemy = targets[i].GetComponentInParent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(damage * finalExplosionMultiplier);
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