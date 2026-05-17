using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직선으로 날아가며 경로상의 적에게 데미지를 주는 검기 투사체.
/// 시각 스프라이트의 실제 크기와 BoxCast 판정 영역을 1:1 동기화합니다.
/// PlayerStats.HasSpecialty("SwordWave_firezone")가 활성이면 궤적에 푸른 마나 불꽃을 남깁니다.
/// </summary>
public class SwordWaveProjectile : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 18f;
    public float maxRange = 6f;

    [Header("판정 설정")]
    public float thickness = 1.5f;
    public bool canHitSameTargetTwice = false;

    [Header("스프라이트 보정")]
    public float nativeSpriteWidth = 6.0f;
    public float nativeSpriteHeight = 6.0f;
    public bool stretchToRange = false;
    [Range(0.3f, 1.0f)] public float hitboxFitRatio = 0.75f;

    [Header("✨ 화염 지대 (SwordWave_firezone 특수효과)")]
    [Tooltip("PlayerStats.HasSpecialty('SwordWave_firezone')가 true일 때만 발동")]
    public float firezoneDuration = 2.0f;
    [Tooltip("궤적 화염 지대의 폭 (검기 두께 대비 배율)")]
    public float firezoneThicknessRatio = 0.7f;
    [Tooltip("궤적 화염 지대의 초당 데미지 = 검기 데미지 * 이 비율 (표 기준 0.1 = 1/10)")]
    public float firezoneDpsRatio = 0.1f;
    [Tooltip("궤적 패치를 스폰하는 간격 (월드 단위 거리)")]
    public float firezonePatchSpacing = 0.7f;

    // 런타임 상태
    private Vector2 direction = Vector2.right;
    private float damage;
    private float traveledDistance;
    private float lastFirezoneSpawnDistance = -999f;

    private float hitWidth;
    private float hitHeight;

    private readonly HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();

    // ✨ [변칙] 귀환하는 칼날 (SwordWave_return)
    [HideInInspector] public Transform casterTransform; // 돌아갈 주인
    private bool isReturning = false;            // 돌아오는 중?
    private bool returnEnabled = false;          // 귀환 특수효과 적용 여부
    private readonly HashSet<Collider2D> returnPhaseHitColliders = new HashSet<Collider2D>();

    public void Init(Vector2 dir, float dmg, float range, float moveSpeed, float boxThickness)
    {
        direction = dir.normalized;
        damage = dmg;
        maxRange = range;
        speed = moveSpeed;
        thickness = boxThickness;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float scaleY = thickness / Mathf.Max(0.001f, nativeSpriteHeight);
        float scaleX = stretchToRange
            ? maxRange / Mathf.Max(0.001f, nativeSpriteWidth)
            : scaleY;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        hitWidth = nativeSpriteWidth * scaleX * hitboxFitRatio;
        hitHeight = nativeSpriteHeight * scaleY * hitboxFitRatio;

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(nativeSpriteWidth, nativeSpriteHeight) * hitboxFitRatio;
        }

        returnEnabled = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("SwordWave_return");
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        if (!isReturning)
        {
            if (traveledDistance + step >= maxRange)
            {
                step = maxRange - traveledDistance;
            }

            if (step > 0f)
            {
                CheckHits(step, false);
                transform.position += (Vector3)(direction * step);
                traveledDistance += step;
                TrySpawnFirezonePatch();
            }

            if (traveledDistance >= maxRange)
            {
                if (returnEnabled && casterTransform != null)
                {
                    isReturning = true;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if (casterTransform == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 toCaster = (Vector2)casterTransform.position - (Vector2)transform.position;
            float distLeft = toCaster.magnitude;

            if (distLeft < 0.5f)
            {
                Destroy(gameObject);
                return;
            }

            direction = toCaster.normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            float moveStep = Mathf.Min(step, distLeft);
            CheckHits(moveStep, true);
            transform.position += (Vector3)(direction * moveStep);

            traveledDistance += moveStep;
            TrySpawnFirezonePatch();
        }
    }

    void CheckHits(float castDistance, bool returnPhase)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(hitWidth, hitHeight),
            angle,
            direction,
            castDistance
        );

        var hitSet = returnPhase ? returnPhaseHitColliders : hitColliders;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (!hit.collider.CompareTag("Enemy")) continue;

            if (!canHitSameTargetTwice && hitSet.Contains(hit.collider))
                continue;

            var enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitSet.Add(hit.collider);
            }
        }
    }

    /// <summary>
    /// 화염 지대 특수효과가 활성이면 일정 간격마다 궤적에 푸른 마나 불꽃 패치를 스폰
    /// </summary>
    void TrySpawnFirezonePatch()
    {
        if (PlayerStats.Instance == null) return;
        if (!PlayerStats.Instance.HasSpecialty("SwordWave_firezone")) return;

        if (traveledDistance - lastFirezoneSpawnDistance < firezonePatchSpacing) return;
        lastFirezoneSpawnDistance = traveledDistance;

        SpawnFirezonePatch(transform.position);
    }

    void SpawnFirezonePatch(Vector3 pos)
    {
        // 푸른 마나 불꽃 패치 — 동적으로 생성 (프리팹 없이)
        var patchGo = new GameObject("ManaFlamePatch");
        patchGo.transform.position = pos;
        patchGo.transform.rotation = transform.rotation;

        // 시각: 단색 디스크 (LineRenderer로 원 그리기) + 살짝 빛나는 코어
        float patchRadius = thickness * firezoneThicknessRatio * 0.5f;

        // 외곽선 — 청색 (마나 불꽃)
        var lrOuter = patchGo.AddComponent<LineRenderer>();
        lrOuter.useWorldSpace = false;
        lrOuter.loop = true;
        lrOuter.positionCount = 16;
        lrOuter.startWidth = 0.06f;
        lrOuter.endWidth = 0.06f;
        lrOuter.material = new Material(Shader.Find("Sprites/Default"));
        lrOuter.startColor = new Color(0.4f, 0.7f, 1f, 0.9f);
        lrOuter.endColor = new Color(0.3f, 0.5f, 1f, 0.9f);
        lrOuter.sortingOrder = 50;
        for (int i = 0; i < lrOuter.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / lrOuter.positionCount;
            lrOuter.SetPosition(i, new Vector3(Mathf.Cos(a) * patchRadius, Mathf.Sin(a) * patchRadius, 0));
        }

        // 내부 코어 (작은 채워진 원 — SpriteRenderer + 흰 픽셀)
        var coreGo = new GameObject("Core");
        coreGo.transform.SetParent(patchGo.transform, false);
        var coreSr = coreGo.AddComponent<SpriteRenderer>();
        coreSr.sprite = GetWhitePixelSprite();
        coreSr.color = new Color(0.6f, 0.85f, 1f, 0.45f);
        coreSr.sortingOrder = 49;
        float coreSize = patchRadius * 1.7f;
        coreGo.transform.localScale = new Vector3(coreSize, coreSize, 1f);

        // 콜라이더 + DOT 효과
        var col = patchGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = patchRadius;

        var dot = patchGo.AddComponent<ManaFlamePatch>();
        dot.duration = firezoneDuration;
        dot.dps = damage * firezoneDpsRatio;
        dot.tickInterval = 0.5f; // 0.5초마다 데미지 틱 (초당 데미지의 절반씩)

        Destroy(patchGo, firezoneDuration);
    }

    private static Sprite cachedWhitePixel;
    private static Sprite GetWhitePixelSprite()
    {
        if (cachedWhitePixel != null) return cachedWhitePixel;
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        cachedWhitePixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedWhitePixel;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.8f, 0.6f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Matrix4x4 rot = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.matrix = rot;
        float w = hitWidth > 0f ? hitWidth : nativeSpriteWidth * hitboxFitRatio;
        float h = hitHeight > 0f ? hitHeight : nativeSpriteHeight * hitboxFitRatio;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(w, h, 0.1f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
