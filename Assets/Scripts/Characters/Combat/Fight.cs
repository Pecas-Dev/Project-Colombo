using UnityEngine;
using ProjectColombo.Core;

namespace ProjectColombo.Combat
{
    public class Fight : MonoBehaviour, IAction
    {
        private Animator animator;
        private ActionSchedueler actionScheduler;

        void Awake()
        {
            animator = GetComponent<Animator>();
            actionScheduler = GetComponent<ActionSchedueler>();
        }

        public void Attack()
        {
            actionScheduler.StartAction(this);
            animator.SetTrigger("attack");
        }

        public void CancelAction()
        {
            animator.ResetTrigger("attack");
            animator.SetTrigger("stopAttack");
        }

        void Hit()
        {
            Debug.Log("Hit");
        }
    }
}
