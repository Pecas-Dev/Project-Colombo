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
        public VisualEffect majorAttackAnt;
        public VisualEffect minorAttackAnt;
        public VisualEffect majorAttack;
        public VisualEffect minorAttack;

        private void Start()
        {
            if (onGettingHitMajor != null || onGettingHitMinor != null)
            {
                CustomEvents.OnDamageDelt += PlayDamageVFX;
            }
        }

        private void PlayDamageVFX(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthManager, int comboLength)
        {
            if (healthManager.gameObject == this.gameObject)
            {
                bool oppositeScale = !sameScale;

                if (scale == GameGlobals.MusicScale.MAJOR)
                {
                    onGettingHitMajor.SetBool("CorrectScale", oppositeScale);
                    onGettingHitMajor.Play();
                }
                else if (scale == GameGlobals.MusicScale.MINOR)
                {
                    onGettingHitMinor.SetBool("CorrectScale", oppositeScale);
                    onGettingHitMinor.Play();
                }
            }
        }

        public void PlayAnticipation()
        {
            GameGlobals.MusicScale scale = GetComponent<EntityAttributes>().currentScale;

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                majorAttackAnt.Play();
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                minorAttackAnt.Play();
            }
        }

        public void PlayAttack()
        {
            GameGlobals.MusicScale scale = GetComponent<EntityAttributes>().currentScale;

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                majorAttack.Play();
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                minorAttack.Play();
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