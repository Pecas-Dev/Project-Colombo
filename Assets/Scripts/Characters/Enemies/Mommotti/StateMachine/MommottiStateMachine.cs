using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Input;
using ProjectColombo.StateMachine.Player;
using UnityEngine;
using static ProjectColombo.StateMachine.Player.PlayerStateMachine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateMachine : StateMachine
    {
        public enum MommottiState { IDLE, PATROL, CHASE, CIRCLE, ATTACK };

        [Header("Component References")]
        public Rigidbody m_Rigidbody;
        public Animator m_Animator;
        public EntityAttributes m_EntityAttributes;
        public MommottiAttributes m_MommottiAttributes;
        public Pathfinding m_PathfindingAlgorythm;

        public MommottiState m_CurrentState;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Animator = GetComponent<Animator>();
            m_EntityAttributes = GetComponent<EntityAttributes>();
            m_MommottiAttributes = GetComponent<MommottiAttributes>();
            m_PathfindingAlgorythm = GetComponent<Pathfinding>();
        }

        void Start()
        {
            LogMissingReferenceErrors();

            //SwitchState(new PlayerMovementState(this));
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

        }

        internal void SetCurrentState(MommottiState newState)
        {
            m_CurrentState = newState;
        }
    }
}