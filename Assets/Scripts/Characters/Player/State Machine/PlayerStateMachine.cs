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
        [SerializeField, ReadOnlyInspector] Rigidbody playerRigidbody;
        public Rigidbody PlayerRigidbody => playerRigidbody;
        [SerializeField, ReadOnlyInspector] Animator playerAnimator;
        public Animator PlayerAnimator => playerAnimator;


        [Header("Script References")]
        [SerializeField, ReadOnlyInspector] Stamina staminaSystem;
        public Stamina StaminaSystem => staminaSystem;

        [SerializeField, ReadOnlyInspector] PlayerAnimator playerAnimatorScript;
        public PlayerAnimator PlayerAnimatorScript => playerAnimatorScript;

        [SerializeField, ReadOnlyInspector] EntityAttributes entityAttributes;
        public EntityAttributes EntityAttributes => entityAttributes;

        [SerializeField, ReadOnlyInspector] Targeter targeter;
        public Targeter Targeter => targeter;

        [SerializeField] Attack[] attacks;
        public Attack[] Attacks => attacks;


        [Header("Player State")]
        [SerializeField, ReadOnlyInspector] private PlayerState currentState;
        public PlayerState CurrentState => currentState;


        [Header("Input Reference")]
        [SerializeField] GameInputSO gameInputSO;
        public GameInputSO GameInputSO => gameInputSO;

        [Header("Isometric Settings")]
        [SerializeField, Range(0.0f, 90.0f)] float angle = 45.0f;
        public float Angle => angle;

        [HideInInspector] public bool isInvunerable = false;
        [HideInInspector] public bool isParrying = false;
        [HideInInspector] public bool tryParrying = false;
        [HideInInspector] public bool isInRoll = false;

        void Awake()
        {
            if (GameInputSO != null)
            {
                GameInputSO.Initialize();
            }

            staminaSystem = GetComponent<Stamina>();
            playerAnimatorScript = GetComponent<PlayerAnimator>();
            entityAttributes = GetComponent<EntityAttributes>();
            targeter = GetComponentInChildren<Targeter>();

            playerRigidbody = GetComponent<Rigidbody>();
            playerAnimator = GetComponent<Animator>();
        }

        void Start()
        {
            LogMissingReferenceErrors();
            GameInputSO.Initialize();
            GameInputSO.EnableAllInputs();
            SwitchState(new PlayerMovementState(this));
        }

        public void Impact(Vector3 direction, float knockbackStrength)
        {
            if (GetComponent<HealthManager>().CurrentHealth <= 0)
            {
                knockbackStrength *= 1.5f;
            }

            PlayerRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
            GetComponent<WeaponHitboxManager>().DisableSwordHitbox(); //disable hitbox
            SwitchState(new PlayerStaggerState(this)); //when interrupt switch to attacking
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

        public void ParryFrameStart()
        {
            isParrying = true;
            tryParrying = true;
        }

        public void ParryFrameStop()
        {
            isParrying = false;
        }

        public void ParryPanaltyStop()
        {
            tryParrying = false;
        }

        public void RollInvincibleFrameStart()
        {
            isInvunerable = true;
        }

        public void RollInvincibleFrameStop()
        {
            isInvunerable = false;
        }
    }
}

