using UnityEngine;

/// <summary>
/// 스킬 이펙트가 원하는 데미지 반지름(worldRadius)에 정확히 맞도록 자동 스케일.
/// 사용법: 이펙트 prefab을 Instantiate한 후 이 컴포넌트를 AddComponent하고 ApplyRadius(r) 호출.
///
/// 핵심 아이디어:
///   - 이펙트 prefab의 SpriteRenderer에서 sprite.bounds.extents를 읽어 native 반지름(world units, scale=1)을 구함
///   - 사용자가 원하는 worldRadius / native 반지름 = 필요한 localScale
///   - 자동으로 transform.localScale 설정
///
/// 이펙트 sprite의 투명 영역이 너무 큰 경우(메테오 화염 같은 케이스):
///   activeRatio를 명시적으로 줄 수 있음 (예: 0.32f = sprite 전체의 32%만 실제 보이는 픽셀)
/// </summary>
public class SkillRangeMatcher : MonoBehaviour
{
    /// <summary>
    /// 이펙트의 시각적 활성 영역 비율 (sprite 전체 대비 실제 보이는 픽셀 비율).
    /// 1.0 = sprite 가장자리가 곧 시각의 끝. 0.5 = sprite의 절반이 투명 영역.
    /// </summary>
    public float activeRatio = 1.0f;

    /// <summary>
    /// 원하는 데미지/시각 반지름(world units)에 맞춰 자동 스케일.
    /// </summary>
    public void ApplyRadius(float worldRadius)
    {
        // SpriteRenderer 검색 (자식 포함)
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            // SpriteRenderer가 없으면 그냥 worldRadius를 localScale로 (1unit이 기본 단위라 가정)
            transform.localScale = new Vector3(worldRadius, worldRadius, 1f);
            return;
        }

        // sprite의 native 반지름 = bounds.extents.x (sprite 단위)
        float nativeHalfWidth = sr.sprite.bounds.extents.x;
        if (nativeHalfWidth < 0.001f) nativeHalfWidth = 0.48f; // fallback

        // sprite의 시각적 활성 반지름 = native * activeRatio
        // 우리가 원하는 것: 시각적 활성 반지름 = worldRadius
        // → 필요한 sprite scale = worldRadius / (native * activeRatio)
        float requiredScale = worldRadius / (nativeHalfWidth * Mathf.Max(0.01f, activeRatio));

        transform.localScale = new Vector3(requiredScale, requiredScale, 1f);

        // CircleCollider2D가 있다면 콜라이더의 월드 반지름도 worldRadius와 일치하도록 보정
        var col = GetComponentInChildren<CircleCollider2D>();
        if (col != null)
        {
            // 월드 반지름 = col.radius * transform.lossyScale.x (lossyScale = parent 적용 후 최종)
            // col.radius를 직접 조정해서 worldRadius와 일치
            float lossy = Mathf.Abs(transform.lossyScale.x);
            if (lossy > 0.001f)
            {
                col.radius = worldRadius / lossy;
            }
        }
    }

    /// <summary>
    /// 가로/세로 비대칭 (부채꼴 형 이펙트) 스케일.
    /// </summary>
    public void ApplyRect(float worldWidth, float worldHeight)
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        float nativeW = sr != null && sr.sprite != null ? sr.sprite.bounds.size.x : 0.96f;
        float nativeH = sr != null && sr.sprite != null ? sr.sprite.bounds.size.y : 0.96f;

        float sx = worldWidth / (Mathf.Max(0.01f, nativeW * activeRatio));
        float sy = worldHeight / (Mathf.Max(0.01f, nativeH * activeRatio));

        transform.localScale = new Vector3(sx, sy, 1f);
    }
}
