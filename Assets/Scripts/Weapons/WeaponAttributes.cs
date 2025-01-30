using UnityEngine;
using System.Collections;

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
            if ((transform.parent.tag == "Enemy" && other.tag == "Player") || (transform.parent.tag == "Player" && other.tag == "Enemy"))
            {
                Debug.Log(other.tag);
                Debug.Log("my tag: " + transform.parent.tag);

                HealthManager targetHealth = other.GetComponent<HealthManager>();

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);

                    Vector3 attackDirection = (other.transform.position - transform.parent.position).normalized; //get direction from user to target
                    attackDirection.y = 0.2f; //could be increased to make the hit entity jump a bit

                    Rigidbody targetRigidbody = other.GetComponent<Rigidbody>();

                    if (targetRigidbody != null)
                    {
                        Debug.Log("Before knockback: " + targetRigidbody.linearVelocity); // Log before applying force

                        targetRigidbody.AddForce(attackDirection * knockback, ForceMode.Impulse);

                        StartCoroutine(LogVelocityNextFrame(targetRigidbody)); // Log velocity after physics update
                    }
                }
            }
        }

        // Coroutine to check velocity in the next physics frame
        private IEnumerator LogVelocityNextFrame(Rigidbody rb)
        {
            yield return new WaitForFixedUpdate(); // Wait until the next physics update
            Debug.Log("After knockback: " + rb.linearVelocity);
        }
    }
}