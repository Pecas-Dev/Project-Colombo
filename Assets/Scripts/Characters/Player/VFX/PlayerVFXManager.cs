using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayerVFXManager : MonoBehaviour
    {
        public VisualEffect rollVFX;
        public VisualEffect stepVFX;
        public VisualEffect majorStunIndicatorVFX;
        public VisualEffect minorStunIndicatorVFX;
        public VisualEffect blockVFX;
        public VisualEffect successfullBlockVFX;

        private void Start()
        {
            CustomEvents.OnSuccessfullParry += PlaySuccessfullParryVFX;
            CustomEvents.OnDamageBlocked += PlaySuccessfullBlockVFX;
        }

        private void PlaySuccessfullBlockVFX(int arg1, GameGlobals.MusicScale arg2, Combat.HealthManager arg3)
        {
            successfullBlockVFX.Play();
        }

        private void PlaySuccessfullParryVFX(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                if (scale == GameGlobals.MusicScale.MINOR)
                {
                    majorStunIndicatorVFX.Play();
                }
                else if (scale == GameGlobals.MusicScale.MAJOR)
                {
                    minorStunIndicatorVFX.Play();
                }
            }
        }

        private void OnDestroy()
        {
            CustomEvents.OnSuccessfullParry -= PlaySuccessfullParryVFX;
            CustomEvents.OnDamageBlocked -= PlaySuccessfullBlockVFX;
        }

        public void PlayRollVFX()
        {
            rollVFX.Play();
        }

        public void PlayStepVFX()
        {
            stepVFX.Play();
        }
    }
}