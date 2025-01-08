using UnityEngine;


namespace ProjectColombo.Combat
{
    public class Fight : MonoBehaviour
    {
        Animator animator;


        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Attack()
        {
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
