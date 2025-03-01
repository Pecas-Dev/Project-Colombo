using Unity.Cinemachine;
using UnityEngine;

namespace ProjectColombo.Camera
{
    public class ScreenShakeManager : MonoBehaviour
    {
        private CinemachineImpulseSource impulseSource;

        void Start()
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void Shake(float force = 1.0f)
        {
            impulseSource.GenerateImpulse(force);
        }
    }
}