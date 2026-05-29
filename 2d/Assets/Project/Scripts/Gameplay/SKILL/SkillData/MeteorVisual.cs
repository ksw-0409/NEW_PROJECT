using UnityEngine;

public class MeteorVisual : MonoBehaviour
{
    public GameObject fireFieldPrefab;
    public GameObject warningCirclePrefab; // ⭐ 떨어지기 전 바닥에 표시할 마법진
    private GameObject warningCircleInstance;
    private float impactDamage;
    private float dotDamage;
    private float duration;
    private float explosionRadius;
    private Vector2 targetDestination;

    private float fallSpeed = 15f; // 떨어지는 속도
    private bool hasExploded = false;
    private float lifeTimer = 0f;
    private bool planetCrashEnabled = false;
    private bool lavaFieldEnabled = false;

    private const float PlanetCrashCenterRadiusRate = 0.35f;
    private const float PlanetCrashExtraDamageMultiplier = 3.0f;
    private const float PlanetCrashKnockbackForce = 12f;
    private const float PlanetCrashStunDuration = 1.0f;
    private const float LavaFieldFixedDuration = 5.0f;

    public void Setup(float dmg, float multiplier, float dur, float rad, Vector2 target)
    {
        impactDamage = dmg;            // 착지 시 즉발 데미지
        dotDamage = dmg * multiplier;  // 장판 데미지 (배율 적용)
        duration = dur;                // 장판 지속시간
        explosionRadius = rad;         // 폭발 및 장판 범위
        targetDestination = target;    // 도달해야 할 바닥 좌표

        // 떨어질 위치에 사전 경고 마법진 즉시 스폰
        // SpawnWarningCircle removed
    }

    private void SpawnWarningCircle()
    {
        if (warningCirclePrefab == null)
        {
            Debug.LogWarning("[MeteorVisual] warningCirclePrefab NULL — 마법진 표시 불가. PlayerSkillController에 할당했는지 확인.");
            return;
        }
        // z=-0.1로 살짝 앞으로 빼서 다른 sprite보다 앞에 그려지게
        Vector3 pos = new Vector3(targetDestination.x, targetDestination.y, -0.1f);
        warningCircleInstance = Instantiate(warningCirclePrefab, pos, Quaternion.identity);

        // sprite native 0.96, 실제 보이는 픽셀은 30%만 사용 (가운데 작은 원)
        // → 월드 지름 = explosionRadius*2가 되려면 scale을 active ratio로 보정
        const float spriteNative = 0.96f;
        const float activeRatio = 0.30f; // aura_icon_11_orange_3 측정값
        float scale = (explosionRadius * 2f) / (spriteNative * activeRatio);
        warningCircleInstance.transform.localScale = new Vector3(scale, scale, 1f);

        var sr = warningCircleInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 500;
            sr.color = new Color(1f, 0.7f, 0.2f, 0.95f);
        }
        Debug.Log($"[MeteorVisual] WarningCircle spawned at {pos} scale={scale} radius={explosionRadius}");
    }

        public void ConfigureSpecialties(bool enablePlanetCrash, bool enableLavaField)
    {
        planetCrashEnabled = enablePlanetCrash;
        lavaFieldEnabled = enableLavaField;
    }

    void Update()
    {
        if (hasExploded) return;

        // 1. 목표 지점을 향해 아래로 이동
        transform.position = Vector3.MoveTowards(transform.position, (Vector3)targetDestination, fallSpeed * Time.deltaTime);
        lifeTimer += Time.deltaTime;

        // 2. 목표 지점에 거의 도달했거나 2초가 지나면 폭발
        if (Vector2.Distance(transform.position, targetDestination) < 0.1f || lifeTimer >= 2.0f)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (planetCrashEnabled)
            CameraShake.Shake(0.7f, 0.55f, 14f);
        else
            CameraShake.ShakePreset(CameraShake.Preset.Epic);

        // 메테오 전체 크기 절반 축소 (이펙트와 판정 둘 다 이게 기준)
        float effectiveRadius = explosionRadius * 0.5f;

        // 1. 즉발 피격 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectiveRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy != null) enemy.TakeDamage(impactDamage);
            }
        }

        if (planetCrashEnabled)
        {
            float centerRadius = Mathf.Max(0.2f, effectiveRadius * PlanetCrashCenterRadiusRate);
            Collider2D[] centerHits = Physics2D.OverlapCircleAll(transform.position, centerRadius);
            foreach (var centerHit in centerHits)
            {
                if (!centerHit.CompareTag("Enemy")) continue;
                EnemyHealth centerEnemy = centerHit.GetComponentInParent<EnemyHealth>();
                if (centerEnemy != null)
                    centerEnemy.TakeDamage(impactDamage * PlanetCrashExtraDamageMultiplier);
            }
            SpawnPlanetCrashFX();
        }

        SkillRangeIndicator.Spawn(
            transform.position,
            effectiveRadius,
            new Color(1f, 0.3f, 0.05f, 0.95f),
            0.7f,
            SkillRangeIndicator.Shape.Circle
        );

        // 2. 불 장판 생성 — SkillRangeMatcher로 자동 동기화
        if (fireFieldPrefab != null)
        {
            GameObject fieldGo = Instantiate(fireFieldPrefab, transform.position, Quaternion.identity);

            // FireField sprite의 시각적 불꽃 활성 비율은 약 32% (이전에 측정)
            var matcher = fieldGo.GetComponent<SkillRangeMatcher>();
            if (matcher == null) matcher = fieldGo.AddComponent<SkillRangeMatcher>();
            matcher.activeRatio = 0.32f;
            matcher.ApplyRadius(effectiveRadius);

            FireField field = fieldGo.GetComponent<FireField>();
            if (field != null)
            {
                field.Setup(duration, dotDamage, 1f, 0f);
            }
        }

        if (lavaFieldEnabled)
        {
            SpawnLavaFieldOverlay();
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // 실제 데미지 영역 = 시각 장판 크기 = explosionRadius * 0.5
        Gizmos.color = new Color(1f, 0.3f, 0.05f, 0.95f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius * 0.5f);
    }

    private void SpawnPlanetCrashFX()
    {
        GameObject pulse = new GameObject("MeteorPlanetCrashFX");
        pulse.transform.position = transform.position;
        SpriteRenderer sr = pulse.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhitePixelSprite();
        sr.color = new Color(1f, 0.5f, 0.1f, 0.55f);
        sr.sortingOrder = 210;
        pulse.transform.localScale = new Vector3(explosionRadius * 2.2f, explosionRadius * 2.2f, 1f);
        Destroy(pulse, 0.25f);
    }

    private void SpawnLavaFieldOverlay()
    {
        GameObject lavaZone = new GameObject("MeteorLavaZoneSpecial");
        lavaZone.transform.position = transform.position;

        CircleCollider2D col = lavaZone.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.3f, explosionRadius);

        FireField field = lavaZone.AddComponent<FireField>();
        float lavaDps = Mathf.Max(impactDamage * 0.1f, (PlayerStats.Instance != null ? PlayerStats.Instance.MagicDamage * 0.1f : 0f));
        field.Setup(LavaFieldFixedDuration, lavaDps, 0.5f, 1.0f);

        LineRenderer lr = lavaZone.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 48;
        lr.startWidth = 0.07f;
        lr.endWidth = 0.07f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0.35f, 0.1f, 0.9f);
        lr.endColor = new Color(1f, 0.2f, 0.05f, 0.9f);
        lr.sortingOrder = 215;

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = (float)i / lr.positionCount;
            float angle = t * Mathf.PI * 2f;
            Vector3 p = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * col.radius;
            lr.SetPosition(i, p);
        }
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