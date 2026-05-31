using UnityEngine;
using TMPro;

/// <summary>
/// 적이 데미지를 받을 때 머리 위에 떠올랐다 사라지는 데미지 숫자.
/// 기본: 흰색. 치명타: 주황-노랑 (큰 글씨 + 더 위로 + 약간 흔들림)
/// World Space 텍스트 (Camera-facing), TextMeshPro 사용.
/// </summary>
public class FloatingDamageText : MonoBehaviour
{
    [Header("애니메이션 파라미터")]
    public float lifetime = 0.9f;          // 총 표시 시간 (초)
    public float floatHeight = 1.2f;       // 위로 떠오르는 거리 (월드 단위)
    public float horizontalDrift = 0.15f;  // 좌우 약간 흔들림

    [Header("색상 (인스펙터에서 조정 가능)")]
    public Color normalColor = new Color(1f, 1f, 1f, 1f);          // 기본: 흰색
    public Color critColorStart = new Color(1f, 0.95f, 0.2f, 1f);  // 치명타: 노랑
    public Color critColorEnd = new Color(1f, 0.5f, 0.05f, 1f);    // 치명타 끝: 주황

    private TextMeshPro tmpText;
    private Vector3 startPos;
    private float driftSign;
    private float elapsed = 0f;
    private bool isCrit = false;
    private float damageValue = 0f;

    /// <summary>
    /// 정적 Spawn — 어디서든 호출하면 데미지 숫자 생성.
    /// </summary>
    public static FloatingDamageText Spawn(Vector3 worldPos, float damage, bool isCritical = false)
    {
        GameObject go = new GameObject("FloatingDamageText");
        go.SetActive(false);
        go.transform.position = worldPos;
        var fdt = go.AddComponent<FloatingDamageText>();
        fdt.damageValue = damage;
        fdt.isCrit = isCritical;
        go.SetActive(true);
        return fdt;
    }

    void Awake()
    {
        // TextMeshPro (3D World Space)
        tmpText = gameObject.AddComponent<TextMeshPro>();
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.enableWordWrapping = false;
        tmpText.sortingOrder = 250; // 위쪽에 표시

        // 살짝 좌우 랜덤
        driftSign = Random.value < 0.5f ? -1f : 1f;
        startPos = transform.position;

        // 초기 텍스트/스타일
        if (isCrit)
        {
            tmpText.text = $"<b>{Mathf.RoundToInt(damageValue)}!</b>";
            tmpText.fontSize = 5.2f;      // 치명타는 더 큼
            tmpText.color = critColorStart;
            // 치명타는 살짝 더 흔들리고 더 위로
            horizontalDrift *= 1.5f;
            floatHeight *= 1.3f;
        }
        else
        {
            tmpText.text = $"{Mathf.RoundToInt(damageValue)}";
            tmpText.fontSize = 4.0f;
            tmpText.color = normalColor;
        }
    }

    void LateUpdate()
    {
        // Editor에서 너무 큰 deltaTime 들어와도 안전
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        elapsed += dt;
        float t = Mathf.Clamp01(elapsed / lifetime);

        // 위로 부드럽게 떠오름 (ease-out)
        float yOffset = Mathf.Sin(t * Mathf.PI * 0.5f) * floatHeight;
        float xOffset = driftSign * Mathf.Sin(t * Mathf.PI) * horizontalDrift;
        transform.position = startPos + new Vector3(xOffset, yOffset, 0);

        // 카메라 향하기 (2D 게임이라 회전 필요 없지만, 만약 카메라 회전 있으면 대응)
        if (Camera.main != null)
        {
            // 2D 카메라라 별도 회전 안 함 — 그냥 0 유지
        }

        // 페이드 & 색상 (치명타는 노랑 → 주황으로 그라데이션)
        float alpha;
        if (t < 0.15f)
        {
            // 초반 빠른 등장 (pop-in)
            alpha = t / 0.15f;
            float popScale = Mathf.Lerp(1.5f, 1f, t / 0.15f);
            transform.localScale = Vector3.one * popScale;
        }
        else if (t < 0.65f)
        {
            // 유지
            alpha = 1f;
            transform.localScale = Vector3.one;
        }
        else
        {
            // 페이드 아웃
            alpha = 1f - ((t - 0.65f) / 0.35f);
            transform.localScale = Vector3.one;
        }

        Color c;
        if (isCrit)
        {
            // 노랑 → 주황 그라데이션
            c = Color.Lerp(critColorStart, critColorEnd, t);
        }
        else
        {
            c = normalColor;
        }
        c.a = Mathf.Clamp01(alpha);
        tmpText.color = c;

        if (elapsed >= lifetime) Destroy(gameObject);
    }
}
