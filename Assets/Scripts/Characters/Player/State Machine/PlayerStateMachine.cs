using ProjectColombo.Combat;
using ProjectColombo.Control;
using ProjectColombo.GameManagement;
using ProjectColombo.GameInputSystem;
using ProjectColombo.Shop;
using ProjectColombo.Inventory;
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
            Parry,
            Dead,
            Shop
        }


        [Header("Component References")]
        public Transform weaponHand;
        public Rigidbody myRigidbody;
        //public Animator myAnimator;
        public Stamina myStamina;
        public PlayerAnimator myPlayerAnimator;
        public EntityAttributes myEntityAttributes;
        public HealthManager myHealthManager;
        public WeaponAttributes myWeaponAttributes;
        public Targeter myTargeter;
        public Attack[] attacks;


        [Header("Player State")]
        public PlayerState currentState;


        //[Header("Input Reference")]
        //public GameInputSO gameInputSO;
        [HideInInspector] public GameInputSO gameInputSO;

        [Header("Isometric Settings")]
        [SerializeField, Range(0.0f, 90.0f)] float angle = 45.0f;
        public float Angle => angle;

        [HideInInspector] public bool isInvunerable = false;
        [HideInInspector] public bool isParrying = false;
        [HideInInspector] public bool tryParrying = false;
        [HideInInspector] public bool isInRoll = false;
        [HideInInspector] public ShopKeeper closeShop = null;

        void Awake()
        {
            //get current selected weapon
            Instantiate(GameManager.Instance.GetMyWeapon(), weaponHand);
            GetComponent<PlayerInventory>().ChangeWeapon(GetComponentInChildren<WeaponAttributes>().name);
            gameInputSO = GameManager.Instance.gameInput;

            myRigidbody = GetComponent<Rigidbody>();
            myStamina = GetComponent<Stamina>();
            myPlayerAnimator = GetComponent<PlayerAnimator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myHealthManager = GetComponent<HealthManager>();
            myWeaponAttributes = GetComponentInChildren<WeaponAttributes>();
            myTargeter = GetComponentInChildren<Targeter>();

            closeShop = null;
        }

        void Start()
        {
            LogMissingReferenceErrors();
            //gameInputSO.Initialize();
            //gameInputSO.EnableAllInputs();
            SwitchState(new PlayerMovementState(this));
        }

        public void Impact(Vector3 direction, float knockbackStrength)
        {
            if (GetComponent<HealthManager>().CurrentHealth <= 0)
            {
                knockbackStrength *= 1.5f;
            }

            myRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
            myWeaponAttributes.GetComponent<Animator>().SetTrigger("Interrupt");
            SwitchState(new PlayerStaggerState(this)); //when interrupt switch to stagger
        }


        void OnDisable()
        {
            gameInputSO.Uninitialize();
        }

        private void FixedUpdate()
        {
            if (myHealthManager.CurrentHealth <= 0 && currentState != PlayerState.Dead)
            {
                SwitchState(new PlayerDeathState(this));
            }
        }

        void LogMissingReferenceErrors()
        {
            if (gameInputSO == null)
            {
                Debug.LogError("GameInput reference is missing!");
            }

            if (myRigidbody == null)
            {
                Debug.LogError("PlayerRigidbody reference is missing!");
            }

            if (myPlayerAnimator == null)
            {
                Debug.LogError("PlayerAnimator reference is missing!");
            }

            if (myEntityAttributes == null)
            {
                Debug.LogError("EntityAttributes reference is missing!");
            }

            if (myStamina == null)
            {
                Debug.LogError("StaminaSystem reference is missing!");
            }

            if (myWeaponAttributes == null)
            {
                Debug.LogError("no weapon in player");
            }

            if (myTargeter == null)
            {
                Debug.LogError("no targeter in player");
            }
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

            if (!TryGetComponent<Stamina>(out _))
            {
                gameObject.AddComponent<Stamina>();
            }

            if (!TryGetComponent<PlayerAnimator>(out _))
            {
                gameObject.AddComponent<PlayerAnimator>();
            }

            if (!TryGetComponent<HealthManager>(out _))
            {
                gameObject.AddComponent<HealthManager>();
            }

            if (!TryGetComponent<Targeter>(out _))
            {
                gameObject.AddComponent<Targeter>();
            }
        }

        internal void SetCurrentState(PlayerState newState)
        {
            currentState = newState;
        }

        public void StartAttack()
        {
            myWeaponAttributes.GetComponent<Animator>().SetTrigger("Attack");
            myWeaponAttributes.GetComponent<Animator>().ResetTrigger("Interrupt");
        }

        public void InterruptAttack()
        {
            myWeaponAttributes.GetComponent<Animator>().ResetTrigger("Attack");
            myWeaponAttributes.GetComponent<Animator>().SetTrigger("Interrupt");
        }

        public void EnterShopState(GameObject shop)
        {
            if (currentState == PlayerState.Movement)
            {
                SwitchState(new PlayerShopState(this, shop));
            }
        }

        public void ExitShopState()
        {
            if (currentState == PlayerState.Shop)
            {
                SwitchState(new PlayerMovementState(this));
            }
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

