using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayerVFXManager : MonoBehaviour
    {
        public GameObject rollVFX;
        public GameObject stepVFX;
        public GameObject stunIndicatorVFX;

        private void Start()
        {
            CustomEvents.OnSuccessfullParry += PlaySuccessfullParryVFX;
        }

        private void PlaySuccessfullParryVFX(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (sameScale)
            {
                stunIndicatorVFX.GetComponent<VisualEffect>().Play();
            }
        }

        private void OnDestroy()
        {
            CustomEvents.OnSuccessfullParry -= PlaySuccessfullParryVFX;
        }

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