using Unity.Cinemachine;
using UnityEngine;

namespace ProjectColombo.Camera
{
    public class CameraZoom : MonoBehaviour
    {
        public CinemachineCamera virtualCamera;
        public float targetSize = 5f;        // Desired orthographic size
        public float zoomSpeed = 2f;         // Speed of zoom

        private bool shouldZoom = false;

        private void Start()
        {
            targetSize = virtualCamera.Lens.OrthographicSize;    
        }

        void Update()
        {
            if (shouldZoom && virtualCamera != null && virtualCamera.Lens.Orthographic)
            {
                float currentSize = virtualCamera.Lens.OrthographicSize;
                virtualCamera.Lens.OrthographicSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * zoomSpeed);

                if (Mathf.Abs(currentSize - targetSize) < 0.1f)
                {
                    virtualCamera.Lens.OrthographicSize = targetSize;
                    shouldZoom = false;
                }
            }
        }

        public void SetNewZoom(float size)
        {
            targetSize = size;
            shouldZoom = true;
        }
    }
}