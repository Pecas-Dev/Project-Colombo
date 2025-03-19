using UnityEngine;


namespace ProjectColombo.Control
{
    public class PlayerAnimator : MonoBehaviour
    {
        const string PARRY_STATE = "Parry";


        Animator animator;


        bool isInParry = false;
        bool isInRoll = false;
        bool isInStagger = false;


        public bool IsInParry => isInParry;
        public bool IsInRoll => isInRoll;
        public bool IsInStagger => isInStagger;


        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void UpdateAnimator(float speed, bool isRolling, bool hasMovementInput)
        {
            animator.SetFloat("speed", isRolling ? 0 : speed);
        }


        // ---------------------------------------------------------
        // Movement
        // ---------------------------------------------------------
        public void PlayMovementAnimation(float transitionTime = 0.1f)
        {
            animator.CrossFadeInFixedTime("Movement", transitionTime);
        }


        // ---------------------------------------------------------
        // Attack
        // ---------------------------------------------------------
        string currentAnimation;
        public void PlayAttackAnimation(string animationName, float transitionDuration)
        {
            currentAnimation = animationName;
            animator.CrossFadeInFixedTime(animationName, transitionDuration);
        }

        public bool FinishedAttack()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1.0f;
        }


        // ---------------------------------------------------------
        // Roll
        // ---------------------------------------------------------
        public void TriggerRoll()
        {
            isInRoll = true;
            animator.SetTrigger("roll");
        }

        //  Animation Event at the end of the Roll
        public void OnRollFinished()
        {
            isInRoll = false;
        }


        // ---------------------------------------------------------
        // Parry
        // ---------------------------------------------------------
        public void TriggerParry(float transitionDuration = 0.2f)
        {
            isInParry = true;
            animator.CrossFadeInFixedTime("Parry", transitionDuration);
        }

        //  Animation Event at the end of the Parry
        public void OnParryFinished()
        {
            isInParry = false;
        }

        // ---------------------------------------------------------
        // Stagger
        // ---------------------------------------------------------
        public void TriggerStagger()
        {
            isInStagger = true;
            animator.SetTrigger("Impact");
        }

        public void ResetStagger()
        {
            isInStagger = false;
        }

        //  Animation Event at the end of the Roll
        public void OnStaggerFinished()
        {
            isInStagger = false;
        }

        // ---------------------------------------------------------
        // Death
        // ---------------------------------------------------------
        public void TriggerDeath()
        {
            animator.SetTrigger("death");
        }

    }
}
