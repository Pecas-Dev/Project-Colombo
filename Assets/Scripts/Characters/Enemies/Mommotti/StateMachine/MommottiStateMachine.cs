using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Input;
using ProjectColombo.StateMachine.Player;
using UnityEngine;
using static ProjectColombo.StateMachine.Player.PlayerStateMachine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateMachine : StateMachine
    {
        public enum MommottiState { PATROL, ALERTED, CHASE, ATTACK };

        [Header("Component References")]
        public Rigidbody myRigidbody;
        public Animator myAnimator;
        public EntityAttributes myEntityAttributes;
        public MommottiAttributes myMommottiAttributes;
        public Pathfinding myPathfindingAlgorythm;
        public WeaponAttributes myWeaponAttributes;

        public MommottiState m_CurrentState;

        private void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myMommottiAttributes = GetComponent<MommottiAttributes>();
            myPathfindingAlgorythm = GetComponent<Pathfinding>();
            myWeaponAttributes = GetComponentInChildren<WeaponAttributes>();
        }

        void Start()
        {
            LogMissingReferenceErrors();

            SwitchState(new MommottiStatePatrol(this));
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
            
            if (myWeaponAttributes == null)
            {
                Debug.Log("Missing Weapon in Mommotti!");
            }

        }

        internal void SetCurrentState(MommottiState newState)
        {
            m_CurrentState = newState;
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
        }
    }
}