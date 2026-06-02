using UnityEngine;

namespace NimbleGames.BackgroundAssets
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target; // The target to follow
        [SerializeField] private float smoothTime = 0.3f; // The time it takes to reach the target position
        private Vector3 velocity = Vector3.zero; // The velocity used by SmoothDamp

        private void LateUpdate()
        {
            if (target != null)
            {
                // Smoothly move the camera towards the target position
                Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
        }
    }
}

