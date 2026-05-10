using UnityEngine;
using System.Collections;

/// <summary>
/// 보스 특수공격 사전 indicator. 가로/세로 막대 형태.
/// 추적 중 (Setup 호출 후) → StopTracking()으로 추적 중단 가능.
/// 자체 destroy 없음 — 보스 코루틴이 Destroy 호출.
/// </summary>
public class RootIndicator : MonoBehaviour
{
    public enum Axis { Horizontal, Vertical }

    private Transform target;
    private Axis axis;
    private float trackDuration;
    private float length;
    private float thickness;

    private SpriteRenderer sr;
    private bool tracking = true;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Setup(Transform target, Axis axis, float lifetime, float length, float thickness)
    {
        this.target = target;
        this.axis = axis;
        this.trackDuration = lifetime;
        this.length = length;
        this.thickness = thickness;
        this.tracking = true;

        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 0.15f, 0.15f, 0.7f);
            sr.sortingOrder = 100;

            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            if (axis == Axis.Horizontal)
                sr.size = new Vector2(length, thickness);
            else
                sr.size = new Vector2(thickness, length);

            sr.transform.localScale = Vector3.one;
            sr.transform.localPosition = Vector3.zero;
            sr.transform.localRotation = Quaternion.identity;
        }
        Debug.Log($"[RootIndicator] Setup OK - axis={axis} length={length} thickness={thickness}");

        StartCoroutine(LifeRoutine());
    }

    /// <summary>추적 중단 — 현재 위치 그대로 고정</summary>
    public void StopTracking()
    {
        tracking = false;
    }

    private IEnumerator LifeRoutine()
    {
        float t = 0f;
        while (t < trackDuration)
        {
            // 추적 단계 — 매 프레임 플레이어 위치로 따라감 (axis별로 한 축만)
            if (tracking && target != null)
            {
                Vector3 pos = transform.position;
                if (axis == Axis.Horizontal)
                {
                    // 가로 막대: 플레이어의 Y축(높이) 따라다니되 X는 무관 (사용자 기획)
                    pos.y = target.position.y;
                }
                else
                {
                    // 세로 막대: 플레이어의 X축 따라다님
                    pos.x = target.position.x;
                }
                transform.position = pos;
            }
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public Vector3 GetCurrentPosition() => transform.position;
}
