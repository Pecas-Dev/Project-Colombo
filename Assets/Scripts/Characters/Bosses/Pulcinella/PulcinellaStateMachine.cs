using ProjectColombo.Enemies.Pathfinding;
using ProjectColombo.Combat;
using UnityEngine;
using ProjectColombo.Enemies;


namespace ProjectColombo.StateMachine.Pulcinella
{
    public class PulcinellaStateMachine : StateMachine
    {
        public enum PulcinellaState { IDLE, MOVE, ATTACK, DEAD };

        [Header("Component References")]
        public Rigidbody myRigidbody;
        public Animator myAnimator;
        public EntityAttributes myEntityAttributes;
        public Pathfinding myPathfindingAlgorythm;
        public HealthManager myHealthManager;

        public PulcinellaState currentState;
        [ReadOnlyInspector] public Transform playerRef;

        private void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myPathfindingAlgorythm = GetComponent<Pathfinding>();
            myHealthManager = GetComponent<HealthManager>();
        }

        void Start()
        {
            LogMissingReferenceErrors();

            playerRef = GameObject.FindGameObjectWithTag("Player").transform;
            SwitchState(new PulcinellaStateIdle(this));
        }

        private void FixedUpdate()
        {
            if (myHealthManager.CurrentHealth <= 0 && currentState != PulcinellaState.DEAD)
            {
                SwitchState(new PulcinellaStateDeath(this));
            }
        }


        public void ApplyKnockback(Vector3 direction, float knockbackStrength, GameGlobals.MusicScale scale)
        {
            if (myHealthManager.CurrentHealth <= 0)
            {
                knockbackStrength *= 1.5f;
            }

            myRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
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

            if (myPathfindingAlgorythm == null)
            {
                Debug.Log("Missing Pathfinding Algorythm in Mommotti!");
            }

            if (myHealthManager == null)
            {
                Debug.Log("Missing HealthManager in Mommotti!");
            }
        }

        internal void SetCurrentState(PulcinellaState newState)
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

            if (!TryGetComponent<Pathfinding>(out _))
            {
                gameObject.AddComponent<Pathfinding>();
            }

            if (!TryGetComponent<HealthManager>(out _))
            {
                gameObject.AddComponent<HealthManager>();
            }
        }
    }
}