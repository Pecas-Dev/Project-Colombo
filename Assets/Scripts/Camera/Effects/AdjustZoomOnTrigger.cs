using Unity.Cinemachine;
using UnityEngine;

namespace ProjectColombo.Camera
{
    public class OrthoZoomTrigger : MonoBehaviour
    {
        public CameraZoom script;
        public float targetSize = 5f;        // Desired orthographic size


        private void Start()
        {
            script = GameObject.Find("PlayerCamera").GetComponentInChildren<CameraZoom>();
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                script.SetNewZoom(targetSize);
            }
        }
    }
}