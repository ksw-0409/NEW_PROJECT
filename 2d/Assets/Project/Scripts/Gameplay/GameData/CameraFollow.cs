using UnityEngine;

// 역할: 카메라가 플레이어를 따라다니도록 고정

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // 플레이어 Transform 연결
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 오프셋

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}