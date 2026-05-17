using UnityEngine;

/// <summary>
/// 카메라가 플레이어를 따라다니도록 + CameraShake 오프셋을 더해줌
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // 플레이어 Transform 연결
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 오프셋

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 basePos = target.position + offset;

        // ⭐ 카메라 흔들림 오프셋 추가 (CameraShake.Instance가 있을 때만)
        if (CameraShake.Instance != null)
        {
            basePos += CameraShake.Instance.CurrentOffset;
        }

        transform.position = basePos;
    }
}
