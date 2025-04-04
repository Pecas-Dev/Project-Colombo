using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
using ProjectColombo.Combat;
using UnityEngine;


namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateMachine : StateMachine
    {
        public enum MommottiState { PATROL, ALERTED, CHASE, ATTACK, DEAD };

        [Header("Component References")]
        public Rigidbody myRigidbody;
        public Animator myAnimator;
        public EntityAttributes myEntityAttributes;
        public MommottiAttributes myMommottiAttributes;
        public Pathfinding myPathfindingAlgorythm;
        public WeaponAttributes myWeaponAttributes;
        public HealthManager myHealthManager;
        public SkinnedMeshRenderer myColorfullSkin;

        public MommottiState currentState;

        //set speed for animator
        Vector3 positionLastFrame;

        //check for attacking
        [HideInInspector] public bool canAttack = false;

        private void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myMommottiAttributes = GetComponent<MommottiAttributes>();
            myPathfindingAlgorythm = GetComponent<Pathfinding>();
            myWeaponAttributes = GetComponentInChildren<WeaponAttributes>();
            myHealthManager = GetComponent<HealthManager>();

        }

        void Start()
        {
            LogMissingReferenceErrors();

            SwitchState(new MommottiStatePatrol(this));
            positionLastFrame = transform.position;
            myPathfindingAlgorythm.gridManager = myMommottiAttributes.myGridManager;
        }

        private void FixedUpdate() // regular update is used in the state machine
        {
            if (myHealthManager.CurrentHealth <= 0 && currentState != MommottiState.DEAD)
            {
                SwitchState(new MommottiStateDeath(this));
            }

            //calculate speed for animator
            Vector3 movementLastFrame = positionLastFrame - transform.position;
            movementLastFrame.y = 0; //ignore height difference
            float currentSpeed = movementLastFrame.magnitude / Time.deltaTime;
            myAnimator.SetFloat("Speed", currentSpeed);
            positionLastFrame = transform.position;

            if (myMommottiAttributes.playerPosition.gameObject.GetComponent<HealthManager>().CurrentHealth <= 0 && currentState != MommottiState.PATROL)
            {
                SwitchState(new MommottiStatePatrol(this));
            }
        }

        public void Impact(Vector3 direction, float knockbackStrength)
        {
            if (myHealthManager.CurrentHealth <= 0)
            {
                knockbackStrength *= 1.5f;
            }

            myRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
            InterruptAttack();
            canAttack = true;
            SwitchState(new MommottiStateStagger(this)); //when attacked switch to attacking
        }

        void LogMissingReferenceErrors()
        {
            if (myRigidbody == null)
            {
                Debug.Log("Missing Rigidbody in Mommotti!");
            }

            if (myAnimator == null)
            {
                Debug.Log("Missing Animator in Mommotti!");
            }

            if (myEntityAttributes == null)
            {
                Debug.Log("Missing Entity Attributes in Mommotti!");
            }

            if (myMommottiAttributes == null)
            {
                Debug.Log("Missing Mommotti Attributes in Mommotti!");
            }            
            
            if (myPathfindingAlgorythm == null)
            {
                Debug.Log("Missing Pathfinding Algorythm in Mommotti!");
            }

            if (myHealthManager == null)
            {
                Debug.Log("Missing HealthManager in Mommotti!");
            }

            if (myWeaponAttributes == null)
            {
                Debug.Log("Missing Weapon in Mommotti!");
            }

        }

        internal void SetCurrentState(MommottiState newState)
        {
            currentState = newState;
        }

        private void Reset()
        {
            if (!TryGetComponent<Rigidbody>(out _))
            {
                gameObject.AddComponent<Rigidbody>();
            }

            if (!TryGetComponent<Animator>(out _))
            {
                gameObject.AddComponent<Animator>();
            }

            if (!TryGetComponent<EntityAttributes>(out _))
            {
                gameObject.AddComponent<EntityAttributes>();
            }

            if (!TryGetComponent<MommottiAttributes>(out _))
            {
                gameObject.AddComponent<MommottiAttributes>();
            }

            if (!TryGetComponent<Pathfinding>(out _))
            {
                gameObject.AddComponent<Pathfinding>();
            }

            if (!TryGetComponent<HealthManager>(out _))
            {
                gameObject.AddComponent<HealthManager>();
            }
        }

        public void StartAttack()
        {
            myWeaponAttributes.GetComponent<Animator>().SetTrigger("Attack");
            myWeaponAttributes.GetComponent<Animator>().ResetTrigger("Interrupt");
        }

        public void InterruptAttack()
        {
            myWeaponAttributes.isAttacking = false;
            myWeaponAttributes.GetComponent<Animator>().ResetTrigger("Attack");
            myWeaponAttributes.GetComponent<Animator>().SetTrigger("Interrupt");
        }

        public void Telegraphing()
        {
            myWeaponAttributes.Telegraphing();
        }
    }
}