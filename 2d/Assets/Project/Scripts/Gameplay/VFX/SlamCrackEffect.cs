using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 내려찍기 땅 균열 이펙트 (오버워치 라인하르트 궁극기 Earthshatter 스타일).
/// 중심에서 방사상으로 4~6개 균열 라인이 뻗어나감 + 안쪽 발광 + 충격파 링 + 가지치기.
/// LightningCircleExplosion과 유사한 패턴 — Awake/InitializeChildren로 즉시 자식 생성, Update로 시간 진행.
/// </summary>
public class SlamCrackEffect : MonoBehaviour
{
    [Header("핵심 파라미터")]
    public float maxRadius = 4f;
    public float duration = 0.7f;
    [Tooltip("메인 균열 라인 개수")]
    public int crackCount = 6;
    [Tooltip("각 균열의 지그재그 segments")]
    public int crackSegments = 8;
    [Tooltip("지그재그 흔들림 강도 (radius 비율)")]
    public float zigzagRatio = 0.06f;
    [Tooltip("균열당 가지치기 개수 (0이면 없음)")]
    public int branchCount = 1;

    [Header("색상 (라인하르트 주황+노랑 발광)")]
    public Color crackOuterColor = new Color(0.15f, 0.05f, 0.02f, 1f); // 어두운 외각 (균열 라인)
    public Color crackInnerColor = new Color(1f, 0.6f, 0.15f, 1f);     // 밝은 안쪽 (발광)
    public Color shockwaveColor = new Color(1f, 0.5f, 0.1f, 0.9f);     // 충격파 링
    public Color glowColor = new Color(1f, 0.55f, 0.1f, 0.30f);        // 중앙 글로우

    private static Sprite cachedRadialGlow;
    private LineRenderer[] crackOuter;
    private LineRenderer[] crackInner;
    private LineRenderer[] branchOuter;  // 가지 외각
    private LineRenderer[] branchInner;  // 가지 안쪽
    private GameObject centerGlow;
    private LineRenderer shockwaveRing;
    private float[] crackAngles;
    private float[] branchAngles;
    private float[] branchAttachT;   // 가지가 메인 라인의 어느 위치에 붙는지 (0~1)
    private int[] branchParent;       // 가지가 어떤 메인 라인에 붙는지 (인덱스)

    private float elapsed = 0f;
    private bool initialized = false;

    [Header("부채꼴 설정")]
    [Tooltip("부채꼴 중심 방향 (단위 벡터). Vector2.zero면 전방향(360도)")]
    public Vector2 fanDirection = Vector2.zero;
    [Tooltip("부채꼴 총 각도(도). 0 이하면 전방향 360도")]
    public float fanAngle = 110f;

    public static SlamCrackEffect Spawn(Vector3 position, float radius = 4f, float duration = 0.7f)
    {
        return Spawn(position, Vector2.zero, 0f, radius, duration);
    }

    public static SlamCrackEffect Spawn(Vector3 position, Vector2 dir, float fanAngleDeg, float radius = 4f, float duration = 0.7f)
    {
        GameObject go = new GameObject("SlamCrackEffect");
        go.SetActive(false); // ⭐ Awake 지연
        go.transform.position = position;
        var fx = go.AddComponent<SlamCrackEffect>();
        fx.maxRadius = radius;
        fx.duration = duration;
        fx.fanDirection = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.zero;
        fx.fanAngle = fanAngleDeg;
        go.SetActive(true); // 이제 Awake 호출 → 올바른 부채꼴 값 반영
        return fx;
    }

    void Awake()
    {
        if (!initialized) InitializeChildren();
    }

    void InitializeChildren()
    {
        if (initialized) return;
        initialized = true;
        Vector3 center = transform.position;

        // === 중앙 발광 글로우 ===
        centerGlow = new GameObject("CenterGlow");
        centerGlow.transform.SetParent(transform, false);
        centerGlow.transform.localPosition = Vector3.zero;
        var sr = centerGlow.AddComponent<SpriteRenderer>();
        sr.sprite = GetRadialGlowSprite();
        sr.color = glowColor;
        sr.sortingOrder = 200;
        centerGlow.transform.localScale = Vector3.zero;

        // === 메인 균열 라인 (외각 + 안쪽 발광, double-line 기법) ===
        crackOuter = new LineRenderer[crackCount];
        crackInner = new LineRenderer[crackCount];
        crackAngles = new float[crackCount];
        // 부채꼴 모드인지 판단
        bool isFan = fanAngle > 0f && fanDirection != Vector2.zero;
        float baseAngleDeg = isFan ? Mathf.Atan2(fanDirection.y, fanDirection.x) * Mathf.Rad2Deg : 0f;
        float halfFan = isFan ? fanAngle * 0.5f : 180f;

        for (int i = 0; i < crackCount; i++)
        {
            // 부채꼴 모드면 [base-halfFan, base+halfFan] 안에 균등 분포, 아니면 360도
            float a;
            if (isFan)
            {
                float t = crackCount == 1 ? 0.5f : (float)i / (crackCount - 1);
                a = baseAngleDeg - halfFan + (fanAngle * t);
                a += Random.Range(-5f, 5f);
            }
            else
            {
                a = (360f / crackCount) * i + Random.Range(-8f, 8f);
            }
            crackAngles[i] = a;

            crackOuter[i] = CreateCrackLine($"CrackOuter_{i}", crackSegments + 1, 0.35f, 0.15f, crackOuterColor, 210);
            crackInner[i] = CreateCrackLine($"CrackInner_{i}", crackSegments + 1, 0.18f, 0.05f, crackInnerColor, 215);
        }

        // === 가지치기 (branches) ===
        int totalBranches = crackCount * branchCount;
        branchOuter = new LineRenderer[totalBranches];
        branchInner = new LineRenderer[totalBranches];
        branchAngles = new float[totalBranches];
        branchAttachT = new float[totalBranches];
        branchParent = new int[totalBranches];
        for (int i = 0; i < totalBranches; i++)
        {
            int parent = i / branchCount;
            branchParent[i] = parent;
            // 메인 라인의 0.3~0.8 지점에 붙음
            branchAttachT[i] = Random.Range(0.30f, 0.80f);
            // 메인 라인 각도에서 ±25~50도 분기
            float sign = (i % 2 == 0) ? 1f : -1f;
            branchAngles[i] = crackAngles[parent] + sign * Random.Range(15f, 30f);
            // ⭐ 부채꼴 모드면 가지가 범위 밖으로 못 나가게 clamp
            if (isFan)
            {
                float minA = baseAngleDeg - halfFan;
                float maxA = baseAngleDeg + halfFan;
                branchAngles[i] = Mathf.Clamp(branchAngles[i], minA, maxA);
            }

            branchOuter[i] = CreateCrackLine($"BranchOuter_{i}", 5, 0.22f, 0.08f, crackOuterColor, 211);
            branchInner[i] = CreateCrackLine($"BranchInner_{i}", 5, 0.10f, 0.03f, crackInnerColor, 216);
        }

        // === 충격파 링 ===
        shockwaveRing = null; // ⭐ 충격파 링/호 제거 — 사용자 요청
    }

    LineRenderer CreateCrackLine(string name, int positionCount, float startWidth, float endWidth, Color color, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;
        lr.positionCount = positionCount;
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = sortOrder;
        // 초기엔 모두 중심에 모여있게 (보이지 않게)
        for (int s = 0; s < positionCount; s++)
            lr.SetPosition(s, transform.position);
        return lr;
    }

    LineRenderer CreateRing(string name, float width, Color color, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 48;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = sortOrder;
        return lr;
    }

    void Update()
    {
        if (!initialized) return;

        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        elapsed += dt;
        float t = Mathf.Clamp01(elapsed / duration);

        // 페이즈 — 균열은 0~0.4까지 확장, 그 후 페이드
        float crackExpand = t < 0.4f
            ? Mathf.Pow(t / 0.4f, 0.6f)        // ease-out 빠른 확장
            : 1f;
        float fade = t < 0.5f
            ? 1f
            : Mathf.Pow(1f - (t - 0.5f) / 0.5f, 1.5f);

        Vector3 center = transform.position;

        // === 중앙 글로우: 빠르게 커지고 페이드 ===
        float glowScale = maxRadius * 0.4f * crackExpand;
        centerGlow.transform.localScale = new Vector3(glowScale, glowScale, 1f);
        var sr = centerGlow.GetComponent<SpriteRenderer>();
        sr.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * fade);

        // === 메인 균열 라인들 — 지그재그로 grow ===
        for (int i = 0; i < crackCount; i++)
        {
            float a = crackAngles[i] * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
            float lineLen = maxRadius * crackExpand;

            DrawZigzagLine(crackOuter[i], center, dir, lineLen, i * 13);
            DrawZigzagLine(crackInner[i], center, dir, lineLen, i * 13);

            // 페이드 적용
            ApplyAlpha(crackOuter[i], fade);
            ApplyAlpha(crackInner[i], fade * 1.0f); // 안쪽 더 밝게
        }

        // === 가지치기 — 메인 라인보다 살짝 늦게 시작 + 짧음 ===
        float branchExpand = t < 0.1f ? 0f : Mathf.Clamp01((crackExpand - 0.2f) / 0.8f);
        for (int i = 0; i < branchOuter.Length; i++)
        {
            int parent = branchParent[i];
            float parentLen = maxRadius * crackExpand;
            // 메인 라인 시작점 (zigzag 적용 안 한 직선 기준 — 단순화)
            float pa = crackAngles[parent] * Mathf.Deg2Rad;
            Vector3 parentDir = new Vector3(Mathf.Cos(pa), Mathf.Sin(pa), 0);
            Vector3 attachPoint = center + parentDir * (parentLen * branchAttachT[i]);

            float ba = branchAngles[i] * Mathf.Deg2Rad;
            Vector3 bDir = new Vector3(Mathf.Cos(ba), Mathf.Sin(ba), 0);
            float bLen = maxRadius * 0.4f * branchExpand;

            DrawZigzagLine(branchOuter[i], attachPoint, bDir, bLen, i * 37);
            DrawZigzagLine(branchInner[i], attachPoint, bDir, bLen, i * 37);
            ApplyAlpha(branchOuter[i], fade);
            ApplyAlpha(branchInner[i], fade);
        }

        // === 충격파 링/호 — 부채꼴 모드면 호, 아니면 풀 링 ===
        bool isFanMode = fanAngle > 0f && fanDirection != Vector2.zero;
        float ringRadius = maxRadius * crackExpand;
        if (shockwaveRing != null)
        {
        shockwaveRing.loop = !isFanMode;
        if (isFanMode)
        {
            float baseRad = Mathf.Atan2(fanDirection.y, fanDirection.x);
            float halfFanRad = fanAngle * 0.5f * Mathf.Deg2Rad;
            for (int i = 0; i < shockwaveRing.positionCount; i++)
            {
                float u = (float)i / (shockwaveRing.positionCount - 1);
                float a = baseRad - halfFanRad + (fanAngle * Mathf.Deg2Rad) * u;
                shockwaveRing.SetPosition(i, center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * ringRadius);
            }
        }
        else
        {
            for (int i = 0; i < shockwaveRing.positionCount; i++)
            {
                float a = ((float)i / shockwaveRing.positionCount) * Mathf.PI * 2f;
                shockwaveRing.SetPosition(i, center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * ringRadius);
            }
        }
        var rc = shockwaveColor;
        rc.a *= fade;
        shockwaveRing.startColor = rc;
        shockwaveRing.endColor = rc;
        shockwaveRing.startWidth = 0.18f * fade + 0.04f;
        shockwaveRing.endWidth = shockwaveRing.startWidth;
        }

        if (elapsed >= duration) Destroy(gameObject);
    }

    /// <summary>중심에서 dir 방향으로 지그재그로 끝까지 그리는 라인</summary>
    void DrawZigzagLine(LineRenderer lr, Vector3 from, Vector3 dir, float length, int seed)
    {
        int segs = lr.positionCount;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0); // 직교 방향
        for (int s = 0; s < segs; s++)
        {
            float u = (float)s / (segs - 1);
            Vector3 along = from + dir * (length * u);
            if (s > 0 && s < segs - 1)
            {
                // 지그재그 — Perlin noise + sign 변경
                float jitter = (Mathf.PerlinNoise((seed + s) * 0.5f, 0.123f) - 0.5f) * 2f;
                along += perp * jitter * length * zigzagRatio;
            }
            lr.SetPosition(s, along);
        }
    }

    void ApplyAlpha(LineRenderer lr, float a)
    {
        var sc = lr.startColor; sc.a = Mathf.Clamp01(sc.a * 0f + a); // start 색 alpha
        var ec = lr.endColor;   ec.a = Mathf.Clamp01(ec.a * 0f + a);
        // 원본 색을 보존하려면: 위 방식 문제. 단순화 — 원본 RGB 유지하고 alpha만 곱
        Color c0 = lr.startColor; c0.a = a;
        Color c1 = lr.endColor;   c1.a = a;
        lr.startColor = c0;
        lr.endColor = c1;
    }

    /// <summary>중심부 발광용 radial gradient sprite</summary>
    static Sprite GetRadialGlowSprite()
    {
        if (cachedRadialGlow != null) return cachedRadialGlow;
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Vector2 cen = new Vector2(size * 0.5f, size * 0.5f);
        float maxR = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), cen);
                float u = Mathf.Clamp01(1f - (d / maxR));
                float a = Mathf.Pow(u, 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        cachedRadialGlow = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
        return cachedRadialGlow;
    }
}