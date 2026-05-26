using UnityEngine;

/// <summary>
/// 10층 보스(공허의 대사제) — 봉인석 기둥.
/// 맵에 랜덤 생성되며, 유도 레이저를 이 기둥에 맞히면 파괴된다.
/// 레이저가 기둥에 명중하면 OnHitByLaser()가 호출되어 파괴 + 보스에게 명중 통보.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SealPillar : MonoBehaviour
{
    public bool IsDestroyed { get; private set; } = false;

    private SpriteRenderer sr;
    private static Sprite _pillarSprite;

    private static Sprite GetPillarSprite()
    {
        if (_pillarSprite != null) return _pillarSprite;
        // 세로로 긴 사각형 기둥 (보라/회색 톤)
        const int W = 48;
        const int H = 96;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float edge = Mathf.Min(x, W - 1 - x) / (W * 0.5f);
                float shade = 0.5f + edge * 0.4f; // 가운데 밝고 가장자리 어둡게
                Color c = new Color(0.5f * shade, 0.35f * shade, 0.65f * shade, 1f);
                // 위쪽에 보라 룬 느낌 하이라이트
                if (y > H * 0.6f && Mathf.Abs(x - W / 2) < W * 0.2f)
                    c = Color.Lerp(c, new Color(0.8f, 0.5f, 1f, 1f), 0.5f);
                tex.SetPixel(x, y, c);
            }
        tex.Apply();
        _pillarSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 48f);
        _pillarSprite.name = "SealPillarSprite";
        return _pillarSprite;
    }

    public void Setup()
    {
        IsDestroyed = false;

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            sr = vis.AddComponent<SpriteRenderer>();
        }
        if (sr.sprite == null) sr.sprite = GetPillarSprite();
        sr.color = Color.white;
        sr.sortingOrder = 30;

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        gameObject.tag = "SealPillar";
    }

    /// <summary>레이저가 명중했을 때 호출 — 기둥 파괴.</summary>
    public void OnHitByLaser()
    {
        if (IsDestroyed) return;
        IsDestroyed = true;

        // 파괴 이펙트 (화면 흔들림)
        if (CameraShake.Instance != null)
            CameraShake.ShakePreset(CameraShake.Preset.Medium);

        Destroy(gameObject);
    }
}
