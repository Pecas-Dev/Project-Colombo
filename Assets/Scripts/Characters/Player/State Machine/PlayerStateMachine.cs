using ProjectColombo.Combat;
using ProjectColombo.Control;
using ProjectColombo.GameInputSystem;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerStateMachine : StateMachine
    {
        public enum PlayerState
        {
            Movement,
            Roll,
            Attack,
            Parry
        }


        [Header("Component References")]
        [field: SerializeField, ReadOnlyInspector] public Rigidbody PlayerRigidbody { get; private set; }
        [field: SerializeField, ReadOnlyInspector] public Animator PlayerAnimator { get; private set; }


        [Header("Script References")]
        [field: SerializeField] public GameInputSO GameInputSO { get; private set; }
        [field: SerializeField, ReadOnlyInspector] public Stamina StaminaSystem { get; private set; }
        [field: SerializeField, ReadOnlyInspector] public PlayerAnimator PlayerAnimatorScript { get; private set; }
        [field: SerializeField, ReadOnlyInspector] public EntityAttributes EntityAttributes { get; private set; }
        [field: SerializeField, ReadOnlyInspector] public Targeter Targeter { get; private set; }
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

            StaminaSystem = GetComponent<Stamina>();
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

        public void Impact(Vector3 direction, float knockbackStrength)
        {
            PlayerRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
            PlayerAnimator.SetTrigger("Impact"); //stop attack animation
            GetComponent<WeaponHitboxManager>().DisableSwordHitbox(); //disable hitbox
            SwitchState(new PlayerMovementState(this)); //when interrupt switch to attacking
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

            if (StaminaSystem == null)
            {
                Debug.LogError("StaminaSystem reference is missing!");
            }
        }

        internal void SetCurrentState(PlayerState newState)
        {
            currentState = newState;
        }
    }
}

