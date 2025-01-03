using UnityEngine;

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
        if (other.gameObject.tag is "Player" or "Enemy" && other.gameObject.tag != GetComponentInParent<EntityAttributes>().gameObject.tag)
        {
            Vector3 attackDirection = other.transform.position - transform.parent.position; //get direction from user to target
            attackDirection.y = .5f; //could be increased to make the hit entity jump a bit

            other.GetComponent<Rigidbody>().AddForce(attackDirection.normalized * knockback, ForceMode.Impulse);
            other.GetComponent<EntityAttributes>().health -= damage;
        }
    }
}
