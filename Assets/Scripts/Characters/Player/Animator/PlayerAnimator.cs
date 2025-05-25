using System.Collections;
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

            animator.speed = 1f;
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
            animator.speed = 1f;
            animator.CrossFadeInFixedTime("Movement", transitionTime);
        }


        // ---------------------------------------------------------
        // Attack
        // ---------------------------------------------------------
        string currentAnimation;
        public void PlayAttackAnimation(string currentCombo, float transitionDuration, float animationSpeed)
        {
            currentAnimation = attackAnimations.GetValueOrDefault(currentCombo, "none");

            if (currentAnimation == "none")
            {
                Debug.Log("no animation found");
                return;
            }

            animator.speed = animationSpeed;
            animator.CrossFadeInFixedTime(currentAnimation, transitionDuration);

            // Optionally reset after the animation duration
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        }


        public bool FinishedAttack()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1.0f)
            {
                animator.speed = 1f;
                return true;
            }

            return false;
        }


        // ---------------------------------------------------------
        // Roll
        // ---------------------------------------------------------
        public void TriggerRoll()
        {
            animator.speed = 1f;
            isInRoll = true;
            animator.SetTrigger("roll");
        }

        //  Animation Event at the end of the Roll
        public void OnRollFinished()
        {
            animator.speed = 1f;
            isInRoll = false;
        }


        // ---------------------------------------------------------
        // Block
        // ---------------------------------------------------------
        public void TriggerBlock()
        {
            animator.speed = 1f;
            animator.SetTrigger("Block");
        }

        // ---------------------------------------------------------
        // Parry
        // ---------------------------------------------------------
        public void TriggerParry(float transitionDuration = 0.2f)
        {
            animator.speed = 1f;
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
            animator.speed = 1f;
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
            animator.speed = 1f;
            animator.Play("Death");
        }

    }
}
