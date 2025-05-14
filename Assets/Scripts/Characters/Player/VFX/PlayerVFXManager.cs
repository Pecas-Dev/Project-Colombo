using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayerVFXManager : MonoBehaviour
    {
        public GameObject rollVFX;
        public GameObject stepVFX;
        public GameObject majorStunIndicatorVFX;
        public GameObject minorStunIndicatorVFX;

        private void Start()
        {
            CustomEvents.OnSuccessfullParry += PlaySuccessfullParryVFX;
        }

        private void PlaySuccessfullParryVFX(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (sameScale)
            {
                if (scale == GameGlobals.MusicScale.MAJOR)
                {
                    majorStunIndicatorVFX.GetComponent<VisualEffect>().Play();
                }
                else if (scale == GameGlobals.MusicScale.MINOR)
                {
                    minorStunIndicatorVFX.GetComponent<VisualEffect>().Play();
                }
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