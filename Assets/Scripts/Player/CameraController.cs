using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float minZoom = 3f;
        [SerializeField, Min(0f)] private float maxZoom = 8f;
        [SerializeField, Min(0f)] private float zoomSpeed = 5f;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0f)
                return;

            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
        }
    }
}
