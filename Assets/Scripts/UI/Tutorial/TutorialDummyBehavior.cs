using ProjectColombo.Combat;
using ProjectColombo.Enemies;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectColombo.Tutorial
{
    public class TutorialDummyBehavior : MonoBehaviour
    {
        EntityAttributes myEntityAttributes;
        HealthManager myHealthManager;
        Animator myAnimator;

        public int maxHealth;
        public int minHealth;
        public bool healWhenLow;
        int lastFrameHealth;

        public GameGlobals.MusicScale myScale;

        private void Start()
        {
            myEntityAttributes = GetComponent<EntityAttributes>();
            myEntityAttributes.SetScale(myScale);
            GetComponent<TextureSwapScale>().SetMaterials(myScale);


            myHealthManager = GetComponent<HealthManager>();
            myHealthManager.AddHealthPoints(maxHealth);
            lastFrameHealth = maxHealth;

            myAnimator = GetComponent<Animator>();
        }

        private void Update()
        {
            myAnimator.ResetTrigger("Impact");
            
            if (lastFrameHealth > myHealthManager.currentHealth)
            {
                myAnimator.SetTrigger("Impact");
                TutorialEvents.EnemyHit();
            }

            lastFrameHealth = myHealthManager.currentHealth;

            if (healWhenLow && myHealthManager.currentHealth < minHealth)
            {
                myHealthManager.Heal(maxHealth);
            }
        }

        public void SwitchScale(GameGlobals.MusicScale newScale)
        {
            myEntityAttributes.SetScale(newScale);
            GetComponent<TextureSwapScale>().SetMaterials(newScale);

            myHealthManager.Heal(maxHealth);
            lastFrameHealth = maxHealth;
        }
    }
}