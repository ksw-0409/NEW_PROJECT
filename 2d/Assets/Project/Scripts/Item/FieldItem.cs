using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public EquipmentData data;

    [Header("Visual")]
    [Tooltip("필드 아이템 표시 크기 (1.5~2.0 권장)")]
    public float itemDisplayScale = 0.9f;

    [Header("Beam Size (월드 유닛)")]
    public float beamWidthCommon  = 0.7f;
    public float beamHeightCommon = 2.0f;
    public float beamWidthRare    = 0.9f;
    public float beamHeightRare   = 2.6f;
    public float beamWidthEpic    = 1.1f;
    public float beamHeightEpic   = 3.2f;
    public float beamWidthUnique  = 1.2f;
    public float beamHeightUnique = 3.5f;
    public float beamWidthLegend  = 1.3f;
    public float beamHeightLegend = 3.8f;

    private SpriteRenderer sr;
    private GameObject beamObj;
    private SpriteRenderer beamSr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(EquipmentData newData)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        data = newData;

        if (data == null)
        {
            Debug.LogError("[FieldItem] Setup: data가 null로 들어옴!");
            return;
        }

        if (data.icon != null)
        {
            sr.sprite = data.icon;
            sr.enabled = true;
            sr.color = Color.white;
            transform.localScale = new Vector3(itemDisplayScale, itemDisplayScale, 1f);
        }
        else
        {
            Debug.LogWarning($"[FieldItem] '{data.itemName}'(grade={data.grade})의 icon이 null. EquipmentManager의 iconEntries 매핑이 비었거나 CSV의 IconName이 안 맞습니다.");
        }

        CreateGradeBeam(data.grade);
    }

    private void CreateGradeBeam(ItemGrade grade)
    {
        if (beamObj != null) Destroy(beamObj);

        Color color = GetGradeColor(grade);
        Vector2 dims = GetGradeBeamDims(grade);

        beamObj = new GameObject("GradeBeam");
        beamObj.transform.SetParent(transform, false);
        beamObj.transform.localPosition = Vector3.zero;
        beamObj.transform.localRotation = Quaternion.identity;

        // 부모 scale을 상쇄 → 빔은 절대 크기로 표시
        Vector3 inv = SafeInverseScale(transform.localScale);
        beamObj.transform.localScale = new Vector3(dims.x * inv.x, dims.y * inv.y, 1f);

        beamSr = beamObj.AddComponent<SpriteRenderer>();
        beamSr.sprite = BuildBeamSprite();
        beamSr.color = color;
        beamSr.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
        beamSr.sortingOrder = (sr != null ? sr.sortingOrder : 0) - 1;
        beamSr.drawMode = SpriteDrawMode.Simple;

        var pulse = beamObj.AddComponent<GradeBeamPulse>();
        pulse.baseColor = color;
    }

    private Vector3 SafeInverseScale(Vector3 s)
    {
        return new Vector3(
            Mathf.Abs(s.x) < 1e-4f ? 1f : 1f / s.x,
            Mathf.Abs(s.y) < 1e-4f ? 1f : 1f / s.y,
            1f);
    }

    private Color GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common:    return new Color(0.90f, 0.90f, 0.90f, 0.70f);
            case ItemGrade.Rare:      return new Color(0.30f, 0.55f, 1.00f, 0.85f);
            case ItemGrade.Epic:      return new Color(0.75f, 0.30f, 1.00f, 0.90f);
            case ItemGrade.Unique:    return new Color(1.00f, 0.35f, 0.70f, 0.92f); // 분홍/마젠타
            case ItemGrade.Legendary: return new Color(1.00f, 0.75f, 0.20f, 0.95f);
            default:                  return new Color(1f, 1f, 1f, 0.70f);
        }
    }

    private Vector2 GetGradeBeamDims(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common:    return new Vector2(beamWidthCommon, beamHeightCommon);
            case ItemGrade.Rare:      return new Vector2(beamWidthRare, beamHeightRare);
            case ItemGrade.Epic:      return new Vector2(beamWidthEpic, beamHeightEpic);
            case ItemGrade.Unique:    return new Vector2(beamWidthUnique, beamHeightUnique);
            case ItemGrade.Legendary: return new Vector2(beamWidthLegend, beamHeightLegend);
            default: return new Vector2(beamWidthCommon, beamHeightCommon);
        }
    }

    private static Sprite _cachedBeamSprite;
    private static Sprite BuildBeamSprite()
    {
        if (_cachedBeamSprite != null) return _cachedBeamSprite;

        const int W = 64;
        const int H = 256;
        const float PPU = 64f;

        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[W * H];
        float centerX = (W - 1) * 0.5f;
        for (int y = 0; y < H; y++)
        {
            float vAlpha = 1f - (float)y / (H - 1);
            vAlpha = Mathf.Pow(vAlpha, 1.4f);

            for (int x = 0; x < W; x++)
            {
                float dx = Mathf.Abs(x - centerX) / centerX;
                float hAlpha = Mathf.Clamp01(1f - dx);
                hAlpha = Mathf.Pow(hAlpha, 1.4f);

                float a = vAlpha * hAlpha;
                pixels[y * W + x] = new Color(1f, 1f, 1f, a);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        _cachedBeamSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), PPU);
        _cachedBeamSprite.name = "GradeBeamSprite";
        return _cachedBeamSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Inventory.Instance == null) return;

        // 인벤토리 가득 찼으면 획득 불가
        if (Inventory.Instance.Items.Count >= 28)
        {
            Debug.Log("[FieldItem] 인벤토리가 가득 찼습니다.");
            return;
        }

        Inventory.Instance.AddItem(data);

        if (ItemManager.Instance != null)
            ItemManager.Instance.RemoveItem(gameObject);

        Destroy(gameObject);
    }
}

public class GradeBeamPulse : MonoBehaviour
{
    public Color baseColor = Color.white;
    public float pulseSpeed = 2.0f;
    public float pulseStrength = 0.30f;
    public float scalePulseStrength = 0.06f;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float seed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        seed = Random.value * 10f;
    }

    void OnEnable()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = (Time.time + seed) * pulseSpeed;
        float aMul = 1f - pulseStrength * 0.5f + Mathf.Sin(t) * pulseStrength * 0.5f;
        if (sr != null)
        {
            Color c = baseColor;
            c.a = baseColor.a * aMul;
            sr.color = c;
        }
        float sMul = 1f + Mathf.Sin(t * 1.3f) * scalePulseStrength;
        transform.localScale = new Vector3(baseScale.x * sMul, baseScale.y, baseScale.z);
    }
}
