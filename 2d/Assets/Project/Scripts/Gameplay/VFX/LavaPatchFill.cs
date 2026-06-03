using UnityEngine;

/// <summary>
/// 반경 안에 도트 sprite를 격자형으로 채워서 용암/화염 지대를 시각적으로 표현.
/// 사용: gameObject에 컴포넌트 추가 → Fill(radius) 호출 → 자동으로 자식 sprite 생성
/// </summary>
public class LavaPatchFill : MonoBehaviour
{
    [Header("Sprite 풀 (랜덤 선택)")]
    public Sprite[] lavaSprites;

    [Header("배치 파라미터")]
    [Tooltip("격자 셀 간격 (작을수록 빽빽)")]
    public float spacing = 0.6f; // 빽빽이 채움 // 큰 sprite에 맞춰 간격 늘림
    [Tooltip("각 셀 위치에 더해질 랜덤 오프셋 (자연스러움)")]
    public float positionJitter = 0.18f;
    [Tooltip("sprite 기본 크기 배율")]
    public float spriteScale = 4.5f; // 빨간 원 채우도록 크게 // 빨간 원 가장자리 닿도록 증가
    [Tooltip("크기 랜덤 범위 (0.8~1.2 등)")]
    public Vector2 scaleVariance = new Vector2(0.7f, 1.3f);

    [Header("색상 / 정렬")]
    public Color tint = new Color(1f, 0.85f, 0.4f, 0.95f);
    public int sortingOrder = 5;
    public string sortingLayer = "Default";

    [Header("페이드")]
    [Tooltip("0이면 즉시 풀 alpha. >0이면 0에서 시작해 이 시간동안 풀 alpha까지 증가")]
    public float fadeInDuration = 0f;
    [Tooltip("0이면 페이드 없음. >0이면 lifetime에 걸쳐 alpha 감소")]
    public float fadeOutDuration = 0f;

    private float elapsed = 0f;
    private bool filled = false;

    /// <summary>피격 반경 안에 sprite를 격자형 + jitter로 채움</summary>
    public void Fill(float radius)
    {
        // 기존 자식 정리 (재호출 안전)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        if (lavaSprites == null || lavaSprites.Length == 0)
        {
            Debug.LogWarning("[LavaPatchFill] lavaSprites 비어있음 — fill 불가");
            return;
        }

        // ⭐ 격자 범위를 radius + spriteHalf 까지 확장 + sprite 가장자리가 원 내부에 있으면 표시
        // (빨간 원 가장자리에 sprite가 닿도록)
        float spritePixelSize = (lavaSprites[0] != null) ? lavaSprites[0].rect.width / lavaSprites[0].pixelsPerUnit : 0.32f;
        float spriteHalf = spritePixelSize * spriteScale * 0.5f;
        float gridMax = radius + spriteHalf;

        int count = 0;
        for (float x = -gridMax; x <= gridMax; x += spacing)
        {
            for (float y = -gridMax; y <= gridMax; y += spacing)
            {
                Vector2 jittered = new Vector2(
                    x + Random.Range(-positionJitter, positionJitter),
                    y + Random.Range(-positionJitter, positionJitter)
                );
                // sprite 중심까지의 거리가 radius 안이면 표시 (가장자리는 원 밖으로 약간 나감 = 채움)
                if (jittered.magnitude > radius) continue;

                var go = new GameObject("LavaTile_" + count);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(jittered.x, jittered.y, 0f);

                float s = spriteScale * Random.Range(scaleVariance.x, scaleVariance.y);
                go.transform.localScale = new Vector3(s, s, 1f);

                // 약간 랜덤 회전 (90도 단위로만 — 픽셀 아트 가독성 유지)
                int rotIdx = Random.Range(0, 4);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, rotIdx * 90f);

                // SpriteRenderer 추가
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = lavaSprites[Random.Range(0, lavaSprites.Length)];

                // 색조 — 살짝 랜덤 brightness
                Color c = tint;
                float bv = Random.Range(0.85f, 1.0f);
                c.r = Mathf.Clamp01(c.r * bv);
                c.g = Mathf.Clamp01(c.g * bv);
                c.b = Mathf.Clamp01(c.b * bv);
                sr.color = c;

                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = sortingOrder;

                count++;
            }
        }
        filled = true;
        elapsed = 0f;

        // ⭐ fadeIn 활성이면 시작 시 alpha=0으로 (점차 나타남)
        if (fadeInDuration > 0f)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var sr = transform.GetChild(i).GetComponent<SpriteRenderer>();
                if (sr == null) continue;
                var c = sr.color; c.a = 0f; sr.color = c;
            }
        }
    }

    void Update()
    {
        if (!filled) return;
        if (fadeInDuration <= 0f && fadeOutDuration <= 0f) return;
        elapsed += Time.deltaTime;

        // fadeIn: 0 → 1 (lifetime의 앞쪽)
        float alphaIn = 1f;
        if (fadeInDuration > 0f && elapsed < fadeInDuration)
        {
            alphaIn = Mathf.Clamp01(elapsed / fadeInDuration);
        }

        // fadeOut: 시간 후반에 0으로 (lifetime 기준 elapsed가 fadeOut 시작 시점 이후)
        float alphaOut = 1f;
        if (fadeOutDuration > 0f)
        {
            // lifetime = fadeIn + sustain + fadeOut 라고 가정 시, fadeOutDuration 끝나면 사라짐
            float totalLife = fadeInDuration + fadeOutDuration;
            if (elapsed > fadeInDuration)
            {
                alphaOut = 1f - Mathf.Clamp01((elapsed - fadeInDuration) / fadeOutDuration);
            }
        }

        float alpha = alphaIn * alphaOut;

        for (int i = 0; i < transform.childCount; i++)
        {
            var sr = transform.GetChild(i).GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            var c = sr.color;
            c.a = tint.a * alpha;
            sr.color = c;
        }
    }
}
