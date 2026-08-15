using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float minZoom = 3f;
        [SerializeField, Min(0f)] private float maxZoom = 8f;
        [SerializeField, Min(0f)] private float zoomSpeed = 5f;

        [Header("Follow")]
        [Tooltip("Player to follow. If left empty, will try to find a GameObject tagged \"Player\".")]
        [SerializeField] private Transform target;
        [Tooltip("Time in seconds for the camera to catch up to the target. 0 = snap instantly.")]
        [SerializeField, Min(0f)] private float followSmoothTime = 0.15f;

        [Header("Bounds")]
        [Tooltip("Optional per-level bounds. When set, the camera view is clamped so it never shows past this area. " +
                 "Leave empty for unbounded levels.")]
        [SerializeField] private Collider2D levelBounds;

        private Camera _camera;
        private Vector3 _followVelocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        public void SetBounds(Collider2D newBounds) => levelBounds = newBounds;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }
        }

        private void Update()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0f)
                return;

            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desired = new Vector3(target.position.x, target.position.y, transform.position.z);

            if (levelBounds != null)
            {
                var halfHeight = _camera.orthographicSize;
                var halfWidth = halfHeight * _camera.aspect;
                var bounds = levelBounds.bounds;

                desired.x = ClampAxis(desired.x, bounds.min.x, bounds.max.x, halfWidth);
                desired.y = ClampAxis(desired.y, bounds.min.y, bounds.max.y, halfHeight);
            }

            transform.position = followSmoothTime <= 0f
                ? desired
                : Vector3.SmoothDamp(transform.position, desired, ref _followVelocity, followSmoothTime);
        }

        private static float ClampAxis(float desired, float min, float max, float halfExtent)
        {
            // If the bounds are narrower than the camera's view on this axis, center on the bounds
            // instead of clamping (avoids min > max from Mathf.Clamp).
            if (max - min <= halfExtent * 2f)
                return (min + max) * 0.5f;

            return Mathf.Clamp(desired, min + halfExtent, max - halfExtent);
        }
    }
}
