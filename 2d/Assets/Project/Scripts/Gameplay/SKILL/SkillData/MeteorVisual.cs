using UnityEngine;

public class MeteorVisual : MonoBehaviour
{
    public GameObject fireFieldPrefab;
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

        // 1. 주변 즉발 데미지 판정 (검은 원 범위만큼)
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
                    if (ai != null)
                        ai.ApplyKnockbackAndStun(transform.position, PlanetCrashKnockbackForce, PlanetCrashStunDuration);
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

        // 2. 기본 불 장판 생성 (기존 메테오 장판)
        if (fireFieldPrefab != null)
        {
            GameObject fieldGo = Instantiate(fireFieldPrefab, transform.position, Quaternion.identity);

            // 장판 크기를 폭발 범위에 맞춤 (보정치 2배 적용)
            float fieldScale = explosionRadius * 2f;
            fieldGo.transform.localScale = new Vector3(fieldScale, fieldScale, 1f);

            FireField field = fieldGo.GetComponent<FireField>();
            if (field != null)
            {
                field.Setup(duration, dotDamage, 1f, 0f);
            }
        }

        // 2-1. 용암 지대 특수 장판을 추가로 생성 (기본 장판과 중복)
        if (lavaFieldEnabled)
        {
            SpawnLavaFieldOverlay();
        }

        // 3. 운석 오브젝트 파괴
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // 에디터에서 착지 지점의 범위를 미리 확인
        Gizmos.color = Color.red;
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