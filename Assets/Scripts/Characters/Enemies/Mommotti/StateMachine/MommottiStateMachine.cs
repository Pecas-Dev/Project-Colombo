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
        public Rigidbody m_Rigidbody;
        public Animator m_Animator;
        public EntityAttributes m_EntityAttributes;
        public MommottiAttributes m_MommottiAttributes;
        public Pathfinding m_PathfindingAlgorythm;
        public WeaponAttributes m_WeaponAttributes;

        public MommottiState m_CurrentState;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Animator = GetComponent<Animator>();
            m_EntityAttributes = GetComponent<EntityAttributes>();
            m_MommottiAttributes = GetComponent<MommottiAttributes>();
            m_PathfindingAlgorythm = GetComponent<Pathfinding>();
            m_WeaponAttributes = GetComponentInChildren<WeaponAttributes>();
        }

        void Start()
        {
            LogMissingReferenceErrors();

            SwitchState(new MommottiStatePatrol(this));
        }

        void LogMissingReferenceErrors()
        {
            if (m_Rigidbody == null)
            {
                Debug.Log("Missing Rigidbody in Mommotti!");
            }

            if (m_Animator == null)
            {
                Debug.Log("Missing Animator in Mommotti!");
            }

            if (m_EntityAttributes == null)
            {
                Debug.Log("Missing Entity Attributes in Mommotti!");
            }

            if (m_MommottiAttributes == null)
            {
                Debug.Log("Missing Mommotti Attributes in Mommotti!");
            }            
            
            if (m_PathfindingAlgorythm == null)
            {
                Debug.Log("Missing Pathfinding Algorythm in Mommotti!");
            }            
            
            if (m_WeaponAttributes == null)
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