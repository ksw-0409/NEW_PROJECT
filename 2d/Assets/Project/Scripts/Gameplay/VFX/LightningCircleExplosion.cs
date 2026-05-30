using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightning_FinalExplosion(방전 폭발) 변칙용 거대 푸른 번개 폭발 이펙트.
/// 구체 형태: 중앙 흰색 코어 + 푸른 본체 + 외곽 글로우 + 회전 아크 + 충격파 링.
/// </summary>
public class LightningCircleExplosion : MonoBehaviour
{
    [Header("핵심 파라미터")]
    public float radius = 4f;
    public float duration = 0.65f;
    public int arcCount = 5;
    public float rotationSpeed = 8f;

    [Header("색상 (참고 이미지: 푸른빛 + 밝게)")]
    public Color coreColor = new Color(1f, 1f, 1f, 1f);
    public Color bodyColor = new Color(0.4f, 0.85f, 1f, 1f);
    public Color ringColor = new Color(0.55f, 0.9f, 1f, 1f);
    public Color arcColor = new Color(0.7f, 0.95f, 1f, 1f);

    private static Sprite cachedRadialGlow;
    private GameObject outerGlow, bodyGlow, core;
    private LineRenderer[] arcs;
    private float[] arcPhases;
    private LineRenderer ring1, ring2;
    private float elapsed = 0f;
    private bool initialized = false;

    public static LightningCircleExplosion Spawn(Vector3 position, float radius = 4f, float duration = 0.65f)
    {
        GameObject go = new GameObject("LightningCircleExplosion");
        go.transform.position = position;
        var fx = go.AddComponent<LightningCircleExplosion>();
        fx.radius = radius;
        fx.duration = duration;
        fx.InitializeChildren(); // ⭐ 즉시 자식 생성
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

        // 외곽 광채
        outerGlow = CreateGlow(center, 0f, new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.5f), 215);
        // 본체 구체
        bodyGlow = CreateGlow(center, 0f, new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.9f), 220);
        // 코어
        core = CreateGlow(center, 0f, coreColor, 235);

        // 회전 호 (LineRenderer 여러 개)
        arcs = new LineRenderer[arcCount];
        arcPhases = new float[arcCount];
        for (int i = 0; i < arcCount; i++)
        {
            var go = new GameObject($"Arc_{i}");
            go.transform.position = center;
            go.transform.SetParent(transform, true);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.32f;
            lr.endWidth = 0.10f;
            lr.startColor = arcColor;
            lr.endColor = new Color(arcColor.r, arcColor.g, arcColor.b, 0.3f);
            lr.positionCount = 12;
            lr.sortingOrder = 230;
            arcs[i] = lr;
            arcPhases[i] = i * (Mathf.PI * 2f / arcCount);
        }

        // 충격파 링 2개
        ring1 = CreateRing(center, 0.20f, ringColor, 225);
        ring2 = CreateRing(center, 0.12f, new Color(ringColor.r, ringColor.g, ringColor.b, 0.7f), 224);
    }

    void Update()
    {
        if (!initialized) return;

        // Editor에서 매우 큰 deltaTime이 들어올 수 있으니 클램프
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        elapsed += dt;
        float t = Mathf.Clamp01(elapsed / duration);

        // 페이즈
        float expand = t < 0.15f ? Mathf.Pow(t / 0.15f, 0.5f) : 1f;
        float fade = t < 0.5f ? 1f : Mathf.Pow(1f - (t - 0.5f) / 0.5f, 1.5f);

        Vector3 center = transform.position;

        // 외곽 광채
        float outerScale = radius * 2.6f * expand;
        outerGlow.transform.localScale = new Vector3(outerScale, outerScale, 1f);
        outerGlow.GetComponent<SpriteRenderer>().color =
            new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.5f * fade);

        // 본체 구체
        float bodyScale = radius * 1.8f * expand;
        bodyGlow.transform.localScale = new Vector3(bodyScale, bodyScale, 1f);
        bodyGlow.GetComponent<SpriteRenderer>().color =
            new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.9f * fade);

        // 코어 펄스
        float pulse = 0.7f + 0.3f * Mathf.Sin(t * Mathf.PI * 2f);
        float coreScale = radius * 0.7f * expand * pulse;
        core.transform.localScale = new Vector3(coreScale, coreScale, 1f);
        core.GetComponent<SpriteRenderer>().color =
            new Color(coreColor.r, coreColor.g, coreColor.b, fade);

        // 회전 호
        float rotation = elapsed * rotationSpeed;
        float arcRadius = radius * 1.55f * expand;
        float arcSpan = Mathf.Lerp(Mathf.PI * 0.6f, Mathf.PI * 0.35f, t);
        for (int i = 0; i < arcCount; i++)
        {
            float startAng = arcPhases[i] + rotation;
            int seg = arcs[i].positionCount;
            for (int s = 0; s < seg; s++)
            {
                float u = (float)s / (seg - 1);
                float a = startAng + (u - 0.5f) * arcSpan;
                Vector3 p = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * arcRadius;
                arcs[i].SetPosition(s, p);
            }
            var cc = new Color(arcColor.r, arcColor.g, arcColor.b, fade);
            arcs[i].startColor = cc;
            arcs[i].endColor = new Color(arcColor.r, arcColor.g, arcColor.b, fade * 0.3f);
        }

        // 충격파 링
        UpdateRing(ring1, center, radius * 1.4f * expand, fade);
        UpdateRing(ring2, center, radius * 1.7f * expand, fade * 0.8f);

        if (elapsed >= duration) Destroy(gameObject);
    }

    GameObject CreateGlow(Vector3 pos, float scale, Color color, int sortOrder)
    {
        var go = new GameObject("Glow");
        go.transform.position = pos;
        go.transform.SetParent(transform, true);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetRadialGlowSprite();
        sr.color = color;
        sr.sortingOrder = sortOrder;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        return go;
    }

    LineRenderer CreateRing(Vector3 center, float width, Color color, int sortOrder)
    {
        var go = new GameObject("Ring");
        go.transform.position = center;
        go.transform.SetParent(transform, true);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 64;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = sortOrder;
        return lr;
    }

    void UpdateRing(LineRenderer lr, Vector3 center, float ringRadius, float fadeAlpha)
    {
        for (int i = 0; i < lr.positionCount; i++)
        {
            float a = ((float)i / lr.positionCount) * Mathf.PI * 2f;
            lr.SetPosition(i, center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * ringRadius);
        }
        var c = lr.startColor;
        c.a = fadeAlpha;
        lr.startColor = c;
        lr.endColor = c;
    }

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
