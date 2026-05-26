using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 전체에서 공유하는 비주얼 이펙트 매니저.
/// prefab 없이 코드만으로 ParticleSystem/LineRenderer/Sprite를 즉석 생성.
///
/// 사용 예:
///   VFXManager.SpawnShieldBreak(player.transform.position);
///   VFXManager.SpawnLevelUp(player.transform.position);
///   VFXManager.SpawnDeathBurst(enemy.transform.position, isElite: false);
/// </summary>
public static class VFXManager
{
    // ============================================================
    // 쉴드 파괴 이펙트 — 청백 충격파 + 파편 파티클
    // ============================================================
    public static void SpawnShieldBreak(Vector3 worldPos)
    {
        // 1. 청백 충격파 (확장하는 링)
        SpawnExpandingRing(worldPos,
            startRadius: 0.4f,
            endRadius: 1.6f,
            color: new Color(0.5f, 0.85f, 1f, 1f),
            duration: 0.45f,
            lineWidth: 0.12f);

        // 2. 사방으로 흩어지는 파편 파티클
        SpawnRadialParticles(worldPos,
            count: 24,
            speed: 4.5f,
            lifetime: 0.6f,
            startSize: 0.18f,
            endSize: 0.0f,
            startColor: new Color(0.7f, 0.95f, 1f, 1f),
            endColor: new Color(0.3f, 0.6f, 1f, 0f));

        // 3. 중심 플래시 (한순간 반짝)
        SpawnFlash(worldPos,
            color: new Color(0.7f, 0.95f, 1f, 0.9f),
            startScale: 0.6f,
            endScale: 1.5f,
            duration: 0.15f);
    }

    // ============================================================
    // 쉴드 재생 이펙트 — 부드러운 등장
    // ============================================================
    public static void SpawnShieldRegen(Vector3 worldPos)
    {
        // 위에서 내려오는 청백 빛
        SpawnFlash(worldPos,
            color: new Color(0.6f, 0.85f, 1f, 0.7f),
            startScale: 1.4f,
            endScale: 0.7f,
            duration: 0.55f);

        // 부드러운 링 수축
        SpawnContractingRing(worldPos,
            startRadius: 1.2f,
            endRadius: 0.55f,
            color: new Color(0.5f, 0.85f, 1f, 0.95f),
            duration: 0.55f,
            lineWidth: 0.08f);
    }

    // ============================================================
    // 레벨업 이펙트 — 황금 빛 기둥 + 별 파티클
    // ============================================================
    public static void SpawnLevelUp(Vector3 worldPos)
    {
        // 위로 솟는 빛 기둥
        SpawnLightPillar(worldPos, height: 4f, duration: 1.2f);

        // 황금 파티클 폭발
        SpawnRadialParticles(worldPos,
            count: 30,
            speed: 3.5f,
            lifetime: 1.2f,
            startSize: 0.20f,
            endSize: 0.02f,
            startColor: new Color(1f, 0.9f, 0.4f, 1f),
            endColor: new Color(1f, 0.6f, 0.1f, 0f),
            withGravity: true);

        // 확장 황금 링
        SpawnExpandingRing(worldPos,
            startRadius: 0.3f,
            endRadius: 2.2f,
            color: new Color(1f, 0.85f, 0.3f, 1f),
            duration: 0.8f,
            lineWidth: 0.16f);

        // 중앙 플래시
        SpawnFlash(worldPos,
            color: new Color(1f, 0.95f, 0.5f, 1f),
            startScale: 0.4f,
            endScale: 2.0f,
            duration: 0.4f);

        CameraShake.ShakePreset(CameraShake.Preset.Medium);
    }

    // ============================================================
    // 플레이어 피격 이펙트 (쉴드 없을 때) — 빨간 깜빡임 + 흔들림
    // ============================================================
    public static void SpawnPlayerHurt(Vector3 worldPos)
    {
        // 빨간 충격파
        SpawnExpandingRing(worldPos,
            startRadius: 0.3f,
            endRadius: 1.0f,
            color: new Color(1f, 0.2f, 0.2f, 0.85f),
            duration: 0.25f,
            lineWidth: 0.08f);

        // 빨간 파편
        SpawnRadialParticles(worldPos,
            count: 12,
            speed: 2.5f,
            lifetime: 0.4f,
            startSize: 0.15f,
            endSize: 0.0f,
            startColor: new Color(1f, 0.3f, 0.3f, 1f),
            endColor: new Color(0.6f, 0.1f, 0.1f, 0f));

        CameraShake.ShakePreset(CameraShake.Preset.Light);
    }

    // ============================================================
    // 적 처치 이펙트 — 작은 폭발
    // ============================================================
    public static void SpawnDeathBurst(Vector3 worldPos, bool isElite = false)
    {
        int particleCount = isElite ? 28 : 14;
        float speed = isElite ? 4f : 2.5f;
        Color startColor = isElite ? new Color(1f, 0.4f, 0.2f, 1f) : new Color(0.85f, 0.7f, 0.5f, 1f);
        Color endColor = isElite ? new Color(0.8f, 0.1f, 0.05f, 0f) : new Color(0.4f, 0.3f, 0.2f, 0f);

        SpawnRadialParticles(worldPos,
            count: particleCount,
            speed: speed,
            lifetime: 0.6f,
            startSize: 0.16f,
            endSize: 0.0f,
            startColor: startColor,
            endColor: endColor,
            withGravity: true);

        SpawnFlash(worldPos,
            color: startColor,
            startScale: 0.3f,
            endScale: isElite ? 1.4f : 0.8f,
            duration: 0.2f);

        if (isElite) CameraShake.ShakePreset(CameraShake.Preset.Medium);
    }

    // ============================================================
    // 골드 획득 이펙트 — 작은 황금 반짝임
    // ============================================================
    public static void SpawnGoldPickup(Vector3 worldPos)
    {
        SpawnRadialParticles(worldPos,
            count: 6,
            speed: 1.5f,
            lifetime: 0.4f,
            startSize: 0.10f,
            endSize: 0.0f,
            startColor: new Color(1f, 0.9f, 0.3f, 1f),
            endColor: new Color(1f, 0.7f, 0.1f, 0f),
            withGravity: true);
    }

    // ============================================================
        // ============================================================
    // 보스 처치 연출 — 화려한 폭죽(다중 색 폭발) + 대형 충격파 + 빛기둥
    // 슬로우 모션과 함께 쓰면 폭죽이 천천히 터지는 연출이 됨
    // ============================================================
    public static void SpawnBossDeath(Vector3 worldPos)
    {
        // 1) 대형 중앙 섬광
        SpawnFlash(worldPos,
            color: new Color(1f, 0.97f, 0.8f, 1f),
            startScale: 0.5f,
            endScale: 4.0f,
            duration: 0.5f);

        // 2) 하늘과 연결된 굵은 빛줄기 여러 개 (메인 연출)
        //    중앙에 가장 굵고 높은 빛, 양옆으로 색이 다른 빛줄기들
        SpawnTallLightBeam(worldPos,                              width: 1.6f, height: 14f, duration: 1.6f, color: new Color(1f, 0.95f, 0.55f, 0.9f));  // 중앙 금빛
        SpawnTallLightBeam(worldPos + new Vector3(-1.1f, 0f, 0f), width: 0.9f, height: 10f, duration: 1.4f, color: new Color(1f, 0.8f, 0.3f, 0.8f));   // 왼쪽 주황
        SpawnTallLightBeam(worldPos + new Vector3( 1.1f, 0f, 0f), width: 0.9f, height: 10f, duration: 1.4f, color: new Color(0.7f, 0.9f, 1f, 0.8f));   // 오른쪽 하늘
        SpawnTallLightBeam(worldPos + new Vector3(-2.0f, 0f, 0f), width: 0.6f, height: 7.5f, duration: 1.2f, color: new Color(0.85f, 0.6f, 1f, 0.75f)); // 보라
        SpawnTallLightBeam(worldPos + new Vector3( 2.0f, 0f, 0f), width: 0.6f, height: 7.5f, duration: 1.2f, color: new Color(1f, 0.95f, 0.7f, 0.75f)); // 노랑

        // 3) 빛줄기 주변으로 위로 흩어지는 약간의 반짝임 (과하지 않게)
        SpawnUpwardParticles(worldPos,
            count: 18,
            upwardSpeed: 7f,
            spread: 3f,
            lifetime: 1.2f,
            startSize: 0.16f,
            endSize: 0.0f,
            startColor: new Color(1f, 0.95f, 0.6f, 1f),
            endColor: new Color(1f, 0.7f, 0.2f, 0f));
    }
    // 위로 솟구쳐 올라가는 빛줄기 파티클 (빛기둥 연출용)
    private static void SpawnUpwardParticles(Vector3 pos, int count, float upwardSpeed, float spread,
                                             float lifetime, float startSize, float endSize,
                                             Color startColor, Color endColor)
    {
        for (int i = 0; i < count; i++)
        {
            // 360도 전방향으로 발사하되, 위쪽(+y)으로 치우쳐진 분수 모양
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float power = Random.Range(0.5f, 1.3f);

            // 수평 성분: 사방으로 퍼짐 (spread로 강도 조절)
            float horiz = Mathf.Cos(angle) * spread * power;
            // 수직 성분: 기본으로 위로 솔구치되, 일부는 앞뒤 기울기(sin)도 섞임
            float up = upwardSpeed * power + Mathf.Sin(angle) * spread * 0.5f;
            // 너무 아래로는 가지 않도록 최소 상승 보장 (분수답게)
            if (up < upwardSpeed * 0.25f) up = upwardSpeed * 0.25f;

            Vector2 velocity = new Vector2(horiz, up);
            Vector3 startPos = pos + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.3f), 0f);
            SpawnSingleParticle(startPos, velocity, lifetime,
                startSize * Random.Range(0.7f, 1.3f),
                endSize, startColor, endColor, withGravity: true);
        }
    }

// 채무자 낙인 활성 시 보라 디버프 오라 (지속)
    // — 자동 활성/비활성은 PlayerDebtMarkVisual이 처리
    // ============================================================
    public static GameObject CreateDebtAura(Transform parent)
    {
        var go = new GameObject("DebtMarkAura");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;

        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 48;
        line.startWidth = 0.06f;
        line.endWidth = 0.06f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 95;

        const float radius = 0.7f;
        for (int i = 0; i < line.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0));
        }

        var c = new Color(0.55f, 0.15f, 0.85f, 0.7f);
        line.startColor = c;
        line.endColor = c;
        return go;
    }

    // ============================================================
    // 내부 헬퍼 함수
    // ============================================================

    // 확장하는 링 (충격파)
    private static void SpawnExpandingRing(Vector3 pos, float startRadius, float endRadius,
                                          Color color, float duration, float lineWidth)
    {
        var go = new GameObject("ExpandRing");
        go.transform.position = pos;
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 40;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 110;
        line.startColor = color;
        line.endColor = color;

        var anim = go.AddComponent<VFXRingAnimator>();
        anim.startRadius = startRadius;
        anim.endRadius = endRadius;
        anim.duration = duration;
        anim.startColor = color;
        anim.endColor = new Color(color.r, color.g, color.b, 0f);
        anim.line = line;
    }

    // 수축하는 링 (쉴드 등장)
    private static void SpawnContractingRing(Vector3 pos, float startRadius, float endRadius,
                                            Color color, float duration, float lineWidth)
    {
        SpawnExpandingRing(pos, startRadius, endRadius, color, duration, lineWidth);
        // 그냥 endRadius < startRadius면 자동으로 수축
    }

    // 사방으로 흩어지는 파티클들
    private static void SpawnRadialParticles(Vector3 pos, int count, float speed, float lifetime,
                                            float startSize, float endSize,
                                            Color startColor, Color endColor,
                                            bool withGravity = false)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.15f, 0.15f);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float spd = speed * Random.Range(0.7f, 1.3f);

            SpawnSingleParticle(pos, dir * spd, lifetime,
                startSize * Random.Range(0.7f, 1.3f),
                endSize, startColor, endColor, withGravity);
        }
    }

    // 단일 파티클 생성
    private static void SpawnSingleParticle(Vector3 pos, Vector2 velocity, float lifetime,
                                           float startSize, float endSize,
                                           Color startColor, Color endColor, bool withGravity)
    {
        var go = new GameObject("VFXParticle");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = startColor;
        sr.sortingOrder = 105;

        var anim = go.AddComponent<VFXParticleAnimator>();
        anim.velocity = velocity;
        anim.lifetime = lifetime;
        anim.startSize = startSize;
        anim.endSize = endSize;
        anim.startColor = startColor;
        anim.endColor = endColor;
        anim.withGravity = withGravity;
        anim.sr = sr;
    }

    // 플래시 — 한 점에 반짝
    private static void SpawnFlash(Vector3 pos, Color color, float startScale, float endScale, float duration)
    {
        var go = new GameObject("VFXFlash");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = color;
        sr.sortingOrder = 115;

        var anim = go.AddComponent<VFXFlashAnimator>();
        anim.startScale = startScale;
        anim.endScale = endScale;
        anim.duration = duration;
        anim.startColor = color;
        anim.endColor = new Color(color.r, color.g, color.b, 0f);
        anim.sr = sr;
    }

    // 빛 기둥 (레벨업)
    private static void SpawnLightPillar(Vector3 pos, float height, float duration)
    {
        var go = new GameObject("VFXLightPillar");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = new Color(1f, 0.9f, 0.3f, 0.7f);
        sr.sortingOrder = 100;

        // 가로는 좁고 세로는 긴 기둥 형태로
        go.transform.localScale = new Vector3(1.2f, height, 1f);

        var anim = go.AddComponent<VFXFlashAnimator>();
        anim.startScale = 1.2f; // 가로 기준 (세로는 height 스케일 별도 적용)
        anim.endScale = 0.4f;
        anim.duration = duration;
        anim.startColor = new Color(1f, 0.9f, 0.3f, 0.85f);
        anim.endColor = new Color(1f, 0.6f, 0.1f, 0f);
        anim.sr = sr;
        anim.preserveYScale = true;
        anim.yScale = height;
    }

    // 하늘과 연결된 굵은 빛줄기 (세로로 길게 솔구치는 직사각형 빔). worldPos 바닥에서 위로 뻗음.
    public static void SpawnTallLightBeam(Vector3 basePos, float width, float height, float duration, Color color, float angleDeg = 0f)
    {
        var go = new GameObject("VFXTallLightBeam");
        // 빔은 바닥(basePos)에서 뻗어나가며, 피봇이 하단 중앙이라 그 점을 중심으로 회전함
        go.transform.position = basePos;
        // angleDeg 만큼 회전 (0 = 위쪽, 양수 = 반시계)
        go.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetBeamSprite();
        sr.color = color;
        sr.sortingOrder = 108;

        // 세로로 길게 (height), 가로는 width
        go.transform.localScale = new Vector3(width, height, 1f);

        var anim = go.AddComponent<VFXBeamAnimator>();
        anim.sr = sr;
        anim.duration = duration;
        anim.baseWidth = width;
        anim.height = height;
        anim.startColor = color;
        anim.endColor = new Color(color.r, color.g, color.b, 0f);
    }

    // 세로 빛줄기용 스프라이트 (아래가 밝고 위로 갈수록 흐릿해지며, 좌우 가장자리 부드러움). 피봇은 하단 중앙.
    private static Sprite _beamSprite;
    private static Sprite GetBeamSprite()
    {
        if (_beamSprite != null) return _beamSprite;
        const int W = 32;
        const int H = 256;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float cx = (W - 1) * 0.5f;
        for (int y = 0; y < H; y++)
        {
            // 세로: 아래(y=0) 불투명, 위(y=H)로 갈수록 투명 (하늘로 트이는 느낌)
            float vy = (float)y / (H - 1);
            float vAlpha = Mathf.Pow(1f - vy, 0.7f);
            for (int x = 0; x < W; x++)
            {
                // 가로: 중앙 밝고 양끝 흐릿 (굵은 빛기둥 느낌)
                float dx = Mathf.Abs(x - cx) / cx;
                float hAlpha = Mathf.Clamp01(1f - dx);
                hAlpha = Mathf.Pow(hAlpha, 1.6f);
                float a = vAlpha * hAlpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        // 피봇을 하단 중앙(0.5, 0)으로 두어 basePos에서 위로 솔아오르게
        _beamSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), 64f);
        _beamSprite.name = "TallBeamSprite";
        return _beamSprite;
    }


    // 원형 sprite 캐시 (코드로 만든 32x32 흰 원)
    private static Sprite _circleSprite;
    private static Sprite GetCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - r + 0.5f;
            float dy = y - r + 0.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            // 부드러운 가장자리
            float a = Mathf.Clamp01(1f - (d / r));
            a = a * a; // 부드러움 강화
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return _circleSprite;
    }
}

// ============================================================
// 애니메이션 컴포넌트들 (각 이펙트의 시간 진행)
// ============================================================

public class VFXRingAnimator : MonoBehaviour
{
    public LineRenderer line;
    public float startRadius, endRadius;
    public float duration;
    public Color startColor, endColor;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        if (t >= 1f) { Destroy(gameObject); return; }

        float radius = Mathf.Lerp(startRadius, endRadius, t);
        Color c = Color.Lerp(startColor, endColor, t);

        for (int i = 0; i < line.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0));
        }
        line.startColor = c;
        line.endColor = c;
    }
}

public class VFXParticleAnimator : MonoBehaviour
{
    public SpriteRenderer sr;
    public Vector2 velocity;
    public float lifetime;
    public float startSize, endSize;
    public Color startColor, endColor;
    public bool withGravity;

    private float timer = 0f;
    private const float Gravity = -6f;

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;
        if (t >= 1f) { Destroy(gameObject); return; }

        if (withGravity)
            velocity.y += Gravity * Time.deltaTime;

        transform.position += (Vector3)(velocity * Time.deltaTime);

        float size = Mathf.Lerp(startSize, endSize, t);
        transform.localScale = new Vector3(size, size, 1f);

        sr.color = Color.Lerp(startColor, endColor, t);
    }
}

public class VFXFlashAnimator : MonoBehaviour
{
    public SpriteRenderer sr;
    public float startScale, endScale;
    public float duration;
    public Color startColor, endColor;
    public bool preserveYScale = false;
    public float yScale = 1f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        if (t >= 1f) { Destroy(gameObject); return; }

        float scale = Mathf.Lerp(startScale, endScale, t);
        if (preserveYScale)
            transform.localScale = new Vector3(scale, yScale, 1f);
        else
            transform.localScale = new Vector3(scale, scale, 1f);

        sr.color = Color.Lerp(startColor, endColor, t);
    }
}

// 하늘로 솟구치는 빛줄기 애니메이션 — 솟아오르며 흔들리고 서서히 사라짐
public class VFXBeamAnimator : MonoBehaviour
{
    public SpriteRenderer sr;
    public float duration;
    public float baseWidth;
    public float height;
    public Color startColor, endColor;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        if (t >= 1f) { Destroy(gameObject); return; }

        // 솟아오르는 느낌: 처음 25%는 위로 빠르게 자라남
        float grow = t < 0.25f ? Mathf.SmoothStep(0f, 1f, t / 0.25f) : 1f;
        // 가로는 살짝 두근거리듯 흔들림
        float widthPulse = baseWidth * (1f + Mathf.Sin(timer * 18f) * 0.12f);
        transform.localScale = new Vector3(widthPulse, height * grow, 1f);

        // 알파: 끝으로 갈수록 사라짐 (뒤쪽 40%에서 페이드아웃)
        float fade = t < 0.6f ? 1f : 1f - ((t - 0.6f) / 0.4f);
        Color c = Color.Lerp(startColor, endColor, 1f - fade);
        c.a = startColor.a * fade;
        sr.color = c;
    }
}