using UnityEngine;

public class FollowEffect : MonoBehaviour
{
    public Transform target;
    public Vector2 dir;
    public float range;

    void Update()
    {
        if (target == null) return;

        transform.position = target.position + (Vector3)(dir * range * 0.7f);
    }
}