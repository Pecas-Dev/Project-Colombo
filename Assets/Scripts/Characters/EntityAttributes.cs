using UnityEngine;

public class EntityAttributes : MonoBehaviour
{
    public int health;
    public float walkSpeed; //for enemies patrol speed
    public float sprintSpeed; //for enemies chase speed
    public float rotationSpeed;
    [HideInInspector] public bool grounded;
    public float jumpForce;
    [HideInInspector] public Animator myAnimator;

    private void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (health <= 0) //here can be stuff that is universal for all entities
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag is "Ground")
        {
            grounded = true;
        }
    }
}
