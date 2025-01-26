using ProjectColombo.Combat;
using ProjectColombo.Control;
using ProjectColombo.Input;

using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerStateMachine : StateMachine
    {
        public enum PlayerState
        {
            Movement,
            Roll,
            Attack
        }


        [Header("Component References")]
        [field: SerializeField] public Rigidbody PlayerRigidbody { get; private set; }
        [field: SerializeField] public Animator PlayerAnimator { get; private set; }


        [Header("Script References")]
        [field: SerializeField] public GameInputSO GameInputSO { get; private set; }
        [field: SerializeField] public PlayerAnimator PlayerAnimatorScript { get; private set; }
        [field: SerializeField] public EntityAttributes EntityAttributes { get; private set; }
        [field: SerializeField] public Targeter Targeter { get; private set; }
        [field: SerializeField] public Attack[] Attacks { get; private set; }


        [Header("Player State")]
        [field: SerializeField, ReadOnlyInspector] private PlayerState currentState;


        public PlayerState CurrentState => currentState;


        void Awake()
        {
            if (GameInputSO != null)
            {
                GameInputSO.Initialize();
            }

            PlayerAnimatorScript = GetComponent<PlayerAnimator>();
            EntityAttributes = GetComponent<EntityAttributes>();
            Targeter = GetComponentInChildren<Targeter>();

            PlayerRigidbody = GetComponent<Rigidbody>();
            PlayerAnimator = GetComponent<Animator>();
        }

        void Start()
        {
            LogMissingReferenceErrors();

            SwitchState(new PlayerMovementState(this));
        }

        void OnDisable()
        {
            GameInputSO.Uninitialize();
        }

        void LogMissingReferenceErrors()
        {
            if (GameInputSO == null)
            {
                Debug.LogError("GameInput reference is missing!");
            }

            if (PlayerRigidbody == null)
            {
                Debug.LogError("PlayerRigidbody reference is missing!");
            }

            if (PlayerAnimatorScript == null)
            {
                Debug.LogError("PlayerAnimator reference is missing!");
            }

            if (EntityAttributes == null)
            {
                Debug.LogError("EntityAttributes reference is missing!");
            }
        }

        internal void SetCurrentState(PlayerState newState)
        {
            currentState = newState;
        }
    }
}

