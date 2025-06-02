using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Masks;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayerVFXManager : MonoBehaviour
    {
        public VisualEffect rollDustVFX;
        public VisualEffect rollLinesVFX;
        public VisualEffect stepVFX;
        public VisualEffect majorStunIndicatorVFX;
        public VisualEffect minorStunIndicatorVFX;
        public VisualEffect blockVFX;
        public VisualEffect successfullBlockVFX;
        public VisualEffect potionVFX;
        public VisualEffect maskAbilityVFX;
        public VisualEffect maskAbilityStunVFX;

        private void Start()
        {
            CustomEvents.OnSuccessfullParry += PlaySuccessfullParryVFX;
            CustomEvents.OnDamageBlocked += PlaySuccessfullBlockVFX;
            CustomEvents.OnPotionUsed += PlayPotionVFX;
            CustomEvents.OnAbilityUsed += PlayAbilityUsedVFX;
        }

        private void PlayAbilityUsedVFX(string obj)
        {
            maskAbilityVFX.Play();

            if (GameManager.Instance.GetComponentInChildren<BaseMask>().maskName == "Mask of the Zanni")
            {
                maskAbilityStunVFX.Play();
            }
        }

        private void PlayPotionVFX()
        {
            potionVFX.Play();
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
            CustomEvents.OnPotionUsed -= PlayPotionVFX;
            CustomEvents.OnAbilityUsed -= PlayAbilityUsedVFX;
        }

        public void PlayRollDustVFX()
        {
            rollDustVFX.Play();
        }

        public void PlayRollLineVFX()
        {
            rollLinesVFX.Play();
        }

        public void PlayStepVFX()
        {
            stepVFX.Play();
        }
    }
}