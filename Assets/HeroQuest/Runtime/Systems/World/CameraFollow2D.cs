using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.12f;
        [SerializeField] private bool clampToBounds;
        [SerializeField] private Bounds worldBounds;

        private Vector3 velocity;
        private Camera followCamera;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        public void SetBounds(Bounds bounds)
        {
            worldBounds = bounds;
            clampToBounds = bounds.size.x > 0f && bounds.size.y > 0f;
        }

        private void Awake()
        {
            followCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + offset;
            desired = ClampToWorldBounds(desired);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        private Vector3 ClampToWorldBounds(Vector3 desired)
        {
            if (!clampToBounds || followCamera == null || !followCamera.orthographic)
            {
                return desired;
            }

            var verticalExtent = followCamera.orthographicSize;
            var horizontalExtent = verticalExtent * followCamera.aspect;
            var minX = worldBounds.min.x + horizontalExtent;
            var maxX = worldBounds.max.x - horizontalExtent;
            var minY = worldBounds.min.y + verticalExtent;
            var maxY = worldBounds.max.y - verticalExtent;

            if (minX <= maxX)
            {
                desired.x = Mathf.Clamp(desired.x, minX, maxX);
            }

            if (minY <= maxY)
            {
                desired.y = Mathf.Clamp(desired.y, minY, maxY);
            }

            return desired;
        }
    }
}
