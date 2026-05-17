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

        // ✨ 메테오 임팩트: 강력한 카메라 흔들림
        //   PlanetCrash 특수효과가 있으면 더 강하게
        if (planetCrashEnabled)
            CameraShake.Shake(0.7f, 0.55f, 14f); // EPIC+
        else
            CameraShake.ShakePreset(CameraShake.Preset.Epic);

        // ⭐ 사전 마법진 제거 (이펙트로 페이드 아웃 대신 즉시 destroy — fire field 외곽선이 인계)
        // warningCircle destroy removed

        // 1. 피격판정 (OverlapCircle explosionRadius)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy != null) enemy.TakeDamage(impactDamage);

                if (planetCrashEnabled)
                {
                    EnemyAI ai = hit.GetComponentInParent<EnemyAI>();
                  // 둘화 추가
                  //   ai.ApplyKnockbackAndStun(transform.position, PlanetCrashKnockbackForce, PlanetCrashStunDuration);
                }
            }
        }

        if (planetCrashEnabled)
        {
            float centerRadius = Mathf.Max(0.35f, explosionRadius * PlanetCrashCenterRadiusRate);
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

        // 2. 레드 폭발 고리 가시화 — 즉발 데미지 영역
        SkillRangeIndicator.Spawn(
            transform.position,
            explosionRadius,
            new Color(1f, 0.3f, 0.05f, 0.95f),
            0.7f,
            SkillRangeIndicator.Shape.Circle
        );

        // 3. 불 장판 생성 — ⭐ FireFieldPrefab의 콜라이더 r=0.48이 월드 r=explosionRadius가 되도록
        // 동시에 장판 하단에 지속적인 레인지 링을 자식으로 추가해서 "이 안에 들어오면 데미지"를 명시
        if (fireFieldPrefab != null)
        {
            GameObject fieldGo = Instantiate(fireFieldPrefab, transform.position, Quaternion.identity);
            const float colliderBaseRadius = 0.48f;
            float fieldScale = explosionRadius / colliderBaseRadius;
            fieldGo.transform.localScale = new Vector3(fieldScale, fieldScale, 1f);

            FireField field = fieldGo.GetComponent<FireField>();
            if (field != null)
            {
                field.Setup(duration, dotDamage, 1f, 0f);
            }

            // ⭐ 장판의 외곽선을 끝까지 유지하는 indicator를 자식으로 추가 (autoDestroy=false, duration=field의 수명)
            var ringGo = new GameObject("FireFieldEdgeRing");
            ringGo.transform.position = transform.position;
            var ind = ringGo.AddComponent<SkillRangeIndicator>();
            ind.shape = SkillRangeIndicator.Shape.Circle;
            ind.radius = explosionRadius;
            ind.edgeColor = new Color(1f, 0.4f, 0.1f, 0.85f);
            ind.fillColor = new Color(1f, 0.4f, 0.1f, 0.06f);
            ind.lineWidth = 0.1f;
            ind.autoDestroy = false;
            // FireField 수명과 동일하게 수동 파괴
            Destroy(ringGo, duration);
        }

        if (lavaFieldEnabled)
        {
            SpawnLavaFieldOverlay();
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // 착지 지점의 실제 판정 범위(OverlapCircle explosionRadius)과 일치
        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
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
        float lavaDps = Mathf.Max(impactDamage * 0.1f, PlayerStats.Instance.MagicDamage * 0.1f);
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