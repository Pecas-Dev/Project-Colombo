using UnityEngine;

namespace ProjectColombo.Objects.VFX
{
    public class SpawnVFXOnDestroy : MonoBehaviour
    {
        public GameObject VFX;

        public void SpawnVFX()
        {
            float rand = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0, rand, 0);
            Instantiate(VFX, transform.position, transform.rotation);
        }
    }
}