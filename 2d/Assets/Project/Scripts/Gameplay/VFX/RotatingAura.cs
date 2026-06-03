using UnityEngine;

/// <summary>
/// 자식 SpriteRenderer를 가진 GameObject에 부착 — 매 프레임 z축 회전.
/// FireFieldPrefab의 aura sprite 등이 빙글빙글 돌아가게.
/// </summary>
public class RotatingAura : MonoBehaviour
{
    [Tooltip("초당 회전 각도 (양수=반시계, 음수=시계)")]
    public float rotationSpeed = 120f;

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
