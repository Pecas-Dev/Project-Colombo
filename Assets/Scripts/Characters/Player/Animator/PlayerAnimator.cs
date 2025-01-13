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
        }

        public void TriggerRoll()
        {
            animator.SetTrigger("roll");
        }

        public void TriggerAttack()
        {
            animator.SetTrigger("attack");
        }

        /*public void OnRollAnimationEnd()
        {
            playerController.EndRoll();
        }*/
    }
}
