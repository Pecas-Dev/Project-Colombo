using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayVFXOnEvent : MonoBehaviour
    {
        public VisualEffect onGettingHitMajor;
        public VisualEffect onGettingHitMinor;

        private void Start()
        {
            if (onGettingHitMajor != null || onGettingHitMinor != null)
            {
                CustomEvents.OnDamageDelt += PlayDamageVFX;
            }
        }

        private void PlayDamageVFX(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthManager, int comboLength)
        {
            if (healthManager.gameObject == this.gameObject)
            {
                if (scale == GameGlobals.MusicScale.MAJOR)
                {
                    onGettingHitMajor.Play();
                }
                else if (scale == GameGlobals.MusicScale.MINOR)
                {
                    onGettingHitMinor.Play();
                }
            }
        }

        private void OnDestroy()
        {
            if (onGettingHitMajor != null || onGettingHitMinor != null)
            {
                CustomEvents.OnDamageDelt -= PlayDamageVFX;
            }
        }
    }
}