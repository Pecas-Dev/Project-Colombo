using System.Collections.Generic;
using UnityEngine;


namespace ProjectColombo.Control
{
    [System.Serializable]
    public class AttackAnimation
    {
        public string comboKey;
        public string animationName;
    }

    public class PlayerAnimator : MonoBehaviour
    {
        Animator animator;

        bool isInParry = false;
        bool isInRoll = false;
        bool isInStagger = false;


        public bool IsInParry => isInParry;
        public bool IsInRoll => isInRoll;
        public bool IsInStagger => isInStagger;

        public List<AttackAnimation> attackCombinations;
        Dictionary<string, string> attackAnimations;


        void Awake()
        {
            animator = GetComponent<Animator>();

            attackAnimations = new();
            foreach (AttackAnimation combo in attackCombinations)
            {
                attackAnimations[combo.comboKey] = combo.animationName;
            }
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
        public void PlayAttackAnimation(string currentCombo, float transitionDuration)
        {

            currentAnimation = attackAnimations.GetValueOrDefault(currentCombo, "none");

            if (currentAnimation == "none")
            {
                Debug.Log("no animation found");
                return;
            }

            animator.CrossFadeInFixedTime(currentAnimation, transitionDuration);
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
        // Block
        // ---------------------------------------------------------
        public void TriggerBlock()
        {
            //animator.SetTrigger("block");
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
