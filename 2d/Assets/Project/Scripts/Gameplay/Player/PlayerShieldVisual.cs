using UnityEngine;

/// <summary>
/// 쉴드 활성 시 Player 주변에 마법 보호막 비주얼을 표시.
/// 3중 레이어 구성:
///   1. 외곽 굵은 링 — 청백 펄스
///   2. 내부 얇은 링 — 반대 방향 회전
///   3. 6개의 다이아몬드 룬 — 외곽에서 천천히 회전
///
/// PlayerStats.OnShieldChanged 이벤트를 구독해서 자동 보이기/숨기기.
/// </summary>
public class PlayerShieldVisual : MonoBehaviour
{
    private PlayerStats stats;

    [Header("비주얼 설정")]
    public float radius = 0.7f;
    public Color shieldColor = new Color(0.4f, 0.85f, 1f, 0.85f);

    // 비주얼 요소들
    private GameObject root;
    private LineRenderer outerLine;
    private LineRenderer innerLine;
    private GameObject[] runeIcons = new GameObject[6];

    private float pulseTimer = 0f;
    private float outerRotTimer = 0f;
    private float innerRotTimer = 0f;
    private float runeRotTimer = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        CreateShieldVisual();
        SetVisible(false);
    }

    void OnEnable()
    {
        if (stats != null) stats.OnShieldChanged += OnShieldChanged;
        Refresh();
    }

    void OnDisable()
    {
        if (stats != null) stats.OnShieldChanged -= OnShieldChanged;
    }

    void OnShieldChanged(int newAmount)
    {
        Refresh();
    }

    void Refresh()
    {
        if (stats == null) { SetVisible(false); return; }
        SetVisible(stats.currentShield > 0);
    }

    public void SetVisible(bool v)
    {
        if (root != null) root.SetActive(v);
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;

        // 색상 펄스 (밝기 호흡)
        pulseTimer += Time.deltaTime * 2.2f;
        float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0~1
        Color pulsed = Color.Lerp(
            new Color(shieldColor.r * 0.7f, shieldColor.g * 0.9f, shieldColor.b, shieldColor.a * 0.7f),
            new Color(shieldColor.r * 1.2f, shieldColor.g, shieldColor.b, shieldColor.a),
            pulse);

        if (outerLine != null)
        {
            outerLine.startColor = pulsed;
            outerLine.endColor = pulsed;
        }
        if (innerLine != null)
        {
            Color inner = new Color(pulsed.r, pulsed.g, pulsed.b, pulsed.a * 0.65f);
            innerLine.startColor = inner;
            innerLine.endColor = inner;
        }

        // 외곽 링은 천천히 시계 방향
        outerRotTimer += Time.deltaTime * 18f;
        // 내부 링은 빠르게 반시계 방향
        innerRotTimer -= Time.deltaTime * 35f;

        // 6개 룬 회전 (외곽 링 따라)
        runeRotTimer += Time.deltaTime * 25f;
        for (int i = 0; i < runeIcons.Length; i++)
        {
            if (runeIcons[i] == null) continue;
            float angle = (i * 60f + runeRotTimer) * Mathf.Deg2Rad;
            float r = radius * 1.05f;
            runeIcons[i].transform.localPosition = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
            runeIcons[i].transform.localRotation = Quaternion.Euler(0, 0, runeRotTimer * 2f);

            var sr = runeIcons[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color rc = Color.Lerp(pulsed, Color.white, 0.4f);
                sr.color = new Color(rc.r, rc.g, rc.b, pulsed.a * 0.9f);
            }
        }
    }

    void CreateShieldVisual()
    {
        root = new GameObject("ShieldVisual");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;

        // 1) 외곽 굵은 링
        var outerGo = new GameObject("OuterRing");
        outerGo.transform.SetParent(root.transform, false);
        outerLine = outerGo.AddComponent<LineRenderer>();
        outerLine.useWorldSpace = false;
        outerLine.loop = true;
        outerLine.positionCount = 64;
        outerLine.startWidth = 0.08f;
        outerLine.endWidth = 0.08f;
        outerLine.material = new Material(Shader.Find("Sprites/Default"));
        outerLine.sortingOrder = 100;
        for (int i = 0; i < outerLine.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / outerLine.positionCount;
            outerLine.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0));
        }

        // 2) 내부 얇은 링 (살짝 더 안쪽, 점선 느낌)
        var innerGo = new GameObject("InnerRing");
        innerGo.transform.SetParent(root.transform, false);
        innerLine = innerGo.AddComponent<LineRenderer>();
        innerLine.useWorldSpace = false;
        innerLine.loop = true;
        innerLine.positionCount = 32; // 적게 해서 보더 느낌
        innerLine.startWidth = 0.03f;
        innerLine.endWidth = 0.03f;
        innerLine.material = new Material(Shader.Find("Sprites/Default"));
        innerLine.sortingOrder = 101;
        float innerRadius = radius * 0.85f;
        for (int i = 0; i < innerLine.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / innerLine.positionCount;
            innerLine.SetPosition(i, new Vector3(Mathf.Cos(a) * innerRadius, Mathf.Sin(a) * innerRadius, 0));
        }

        // 3) 6개 다이아몬드 룬 (외곽 링 따라 회전)
        for (int i = 0; i < 6; i++)
        {
            var rune = new GameObject("Rune_" + i);
            rune.transform.SetParent(root.transform, false);
            var sr = rune.AddComponent<SpriteRenderer>();
            sr.sprite = GetDiamondSprite();
            sr.color = shieldColor;
            sr.sortingOrder = 102;
            rune.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            runeIcons[i] = rune;
        }
    }

    // 다이아몬드 모양 sprite (32x32)
    private static Sprite _diamondSprite;
    private static Sprite GetDiamondSprite()
    {
        if (_diamondSprite != null) return _diamondSprite;

        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = size * 0.5f, cy = size * 0.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // 다이아몬드: |x| + |y| <= r 일 때 채움
            float dx = Mathf.Abs(x - cx + 0.5f);
            float dy = Mathf.Abs(y - cy + 0.5f);
            float dist = dx + dy;
            const float maxR = 12f;
            const float edgeStart = 9f;

            float a = 0f;
            if (dist < edgeStart) a = 1f;             // 안쪽 채움
            else if (dist < maxR)                      // 가장자리 페이드
                a = 1f - (dist - edgeStart) / (maxR - edgeStart);

            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        _diamondSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return _diamondSprite;
    }
}
