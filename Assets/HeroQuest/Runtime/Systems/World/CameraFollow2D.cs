using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.12f;

        private Vector3 velocity;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
