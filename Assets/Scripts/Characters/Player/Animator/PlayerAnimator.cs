using UnityEngine;

namespace ProjectColombo.Control
{
    public class PlayerAnimator : MonoBehaviour
    {
        Animator animator;
        PlayerController playerController;

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
        }

        public void UpdateAnimator(float speed, bool isRolling, bool hasMovementInput)
        {
            animator.SetFloat("speed", isRolling ? 0 : speed);

            if (!hasMovementInput && !isRolling)
            {
                float decelerationRate = playerController.GetComponent<EntityAttributes>().deceleration * Time.deltaTime;
                float currentSpeed = Mathf.Max(0, speed - decelerationRate);
                animator.SetFloat("speed", currentSpeed);
            }
        }

        public void TriggerRoll()
        {
            animator.SetTrigger("roll");
        }

        public void OnRollAnimationEnd()
        {
            playerController.EndRoll();
        }
    }
}
