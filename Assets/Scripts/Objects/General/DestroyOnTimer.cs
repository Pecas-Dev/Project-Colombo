using UnityEngine;

namespace ProjectColombo.Objects.VFX
{
    public class DestroyOnTimer : MonoBehaviour
    {
        public float timeInSeconds;

        private void Start()
        {
            Destroy(this.gameObject, timeInSeconds);
        }
    }
}