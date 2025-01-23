using UnityEngine;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        public int damage;
        public float knockback;
        public float cooldown;
        float currentTimer;
        public float reach;
        public Collider hitbox;
        [HideInInspector] public bool attack;
        [HideInInspector] public Animator myAnimator;

        private void Start()
        {
            myAnimator = GetComponent<Animator>();
            currentTimer = 0;
            hitbox.enabled = attack;
        }

        private void Update()
        {
            hitbox.enabled = attack;

            if (attack)
            {
                if (currentTimer == 0)
                {
                    myAnimator.SetTrigger("Attack");
                }

                currentTimer += Time.deltaTime;

                if (currentTimer >= cooldown)
                {
                    attack = false;
                    currentTimer = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // I left this outside the if statemnet below while we test that all behaviours are working, then we can specify which exact entities we want that are affected. 

            //---------------------------------------------------------------------------------------------------------------------
            HealthManager targetHealth = other.GetComponent<HealthManager>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);

                Vector3 attackDirection = other.transform.position - transform.parent.position; //get direction from user to target
                attackDirection.y = .5f; //could be increased to make the hit entity jump a bit

                other.GetComponent<Rigidbody>().AddForce(attackDirection.normalized * knockback, ForceMode.Impulse);
            }
            //---------------------------------------------------------------------------------------------------------------------

            if (other.gameObject.tag is "Player" or "Enemy" && other.gameObject.tag != GetComponentInParent<EntityAttributes>().gameObject.tag)
            {

                //other.GetComponent<EntityAttributes>().health -= damage;
            }
        }
    }
}