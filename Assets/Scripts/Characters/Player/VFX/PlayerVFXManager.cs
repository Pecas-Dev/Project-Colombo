using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayerVFXManager : MonoBehaviour
    {
        public GameObject rollVFX;
        public GameObject stepVFX;


        public void PlayRollVFX()
        {
            rollVFX.GetComponent<VisualEffect>().Play();
        }

        public void PlayStepVFX()
        {
            stepVFX.GetComponent<VisualEffect>().Play();
        }
    }
}