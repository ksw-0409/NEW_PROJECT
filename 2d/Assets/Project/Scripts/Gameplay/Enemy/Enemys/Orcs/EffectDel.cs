using UnityEngine;

public class EffectDel : MonoBehaviour
{
    [SerializeField] private float destroyTime = 0.3f; // 사라질 시간 (초)
    void Start()
    {
        // 생성되자마자 destroyTime 후에 삭제 예약
        Destroy(gameObject, destroyTime);
    }
}
