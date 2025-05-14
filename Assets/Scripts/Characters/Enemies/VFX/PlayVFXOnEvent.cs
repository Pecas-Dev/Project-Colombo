using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Mommotti;
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
                bool oppositeScale = healthManager.gameObject.GetComponent<EntityAttributes>().currentScale != scale;

                if (scale == GameGlobals.MusicScale.MAJOR)
                {
                    onGettingHitMajor.SetBool("CorrectScale", oppositeScale);
                    onGettingHitMajor.Play();
                }
                else if (scale == GameGlobals.MusicScale.MINOR)
                {
                    onGettingHitMajor.SetBool("CorrectScale", oppositeScale);
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