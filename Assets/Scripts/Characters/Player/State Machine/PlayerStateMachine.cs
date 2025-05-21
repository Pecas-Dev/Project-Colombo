using ProjectColombo.Combat;
using ProjectColombo.Control;
using ProjectColombo.GameManagement;
using ProjectColombo.GameInputSystem;
using ProjectColombo.Shop;
using ProjectColombo.Inventory;
using UnityEngine;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement.Stats;
using System.Collections;


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
            Shop,
            Block
        }


        [Header("Component References")]
        public Transform weaponHand;
        public Rigidbody myRigidbody;
        public Stamina myStamina;
        public PlayerAnimator myPlayerAnimator;
        public EntityAttributes myEntityAttributes;
        public HealthManager myHealthManager;
        public WeaponAttributes myWeaponAttributes;
        public Targeter myTargeter;
        public PlayerInventory myPlayerInventory;


        [Header("Player State")]
        public PlayerState currentState;

        [HideInInspector] public GameInputSO gameInputSO;

        [Header("Isometric Settings")]
        [SerializeField, Range(0.0f, 90.0f)] float angle = 45.0f;
        public float Angle => angle;

        [HideInInspector] public bool isBlocking = false;
        [HideInInspector] public bool isInvunerable = false;
        //[HideInInspector] 
        public bool isParrying = false;
        [HideInInspector] public bool tryParrying = false;
        [HideInInspector] public bool isInRoll = false;
        [HideInInspector] public bool comboWindowOpen = false;
        [HideInInspector] public string currentComboString = "";
        [HideInInspector] public ShopKeeper closeShop = null;
        bool activateCharmsAndMask = false;

        void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myStamina = FindFirstObjectByType<Stamina>();
            myPlayerAnimator = GetComponent<PlayerAnimator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myHealthManager = GetComponent<HealthManager>();
            myTargeter = GetComponentInChildren<Targeter>();
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();

            closeShop = null;
        }

        private void OnDestroy()
        {
            CustomEvents.OnSuccessfullParry -= ApplyParryIFrames;
        }

        void Start()
        {
            LogMissingReferenceErrors();
            SwitchState(new PlayerMovementState(this));
            CustomEvents.OnSuccessfullParry += ApplyParryIFrames;
            //get current selected weapon
            SwapWeapon();
        }

        private void ApplyParryIFrames(GameGlobals.MusicScale scale, bool sameScale)
        {
            StartCoroutine(SetInvincible(0.1f));
        }

        IEnumerator SetInvincible(float duration)
        {
            isInvunerable = true;
            yield return new WaitForSeconds(duration);
            isInvunerable = true;
        }

        public void ApplyKnockback(Vector3 direction, float knockbackStrength)
        {
            if (GetComponent<HealthManager>().CurrentHealth <= 0)
            {
                knockbackStrength *= 1.5f;
            }

            myRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
        }

        public void SetStaggered()
        {
            if (currentState == PlayerState.Block) return;

            myWeaponAttributes.DisableWeaponHitbox();
            SwitchState(new PlayerStaggerState(this)); //when interrupt switch to stagger
        }

        void OnDisable()
        {
            gameInputSO.Uninitialize();
        }

        private void FixedUpdate()
        {
            if (!activateCharmsAndMask)
            {
                myPlayerInventory.ActivateMask();
                myPlayerInventory.ActivateCharms();
                activateCharmsAndMask = true;
            }

            if (myHealthManager.CurrentHealth <= 0 && currentState != PlayerState.Dead)
            {
                SwitchState(new PlayerDeathState(this));
            }

            if (gameInputSO.UsePotionPressed)
            {
                myPlayerInventory.UsePotion();
            }

            if (gameInputSO.UseCharmAbilityPressed)
            {
                myPlayerInventory.UseCharmAbility();
            }

            if (gameInputSO.UseSpecialAbilityPressed)
            {
                myPlayerInventory.UseMaskAbility();
            }

            gameInputSO.ResetAllInputs();
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

            if (myTargeter == null)
            {
                Debug.LogError("no targeter in player");
            }

            if (myPlayerAnimator == null)
            {
                Debug.LogError("no playerinventory");
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
            myWeaponAttributes.EnableWeaponHitbox();
        }

        public void InterruptAttack()
        {
            myWeaponAttributes.DisableWeaponHitbox();
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

        void SwapWeapon()
        {
            Instantiate(GameManager.Instance.gameObject.GetComponent<GlobalStats>().GetMyWeapon(), weaponHand);
            myWeaponAttributes = GetComponentInChildren<WeaponAttributes>();
            gameInputSO = GameManager.Instance.gameInput;
        }

        public void OpenComboWindow()
        {
            if (currentComboString.Length <= 2)
            {
                //Debug.Log("combo Window open");
                comboWindowOpen = true;
            }
            else
            {
                //Debug.Log("third combo already. no opening");
            }
        }

        public void CloseComboWindow()
        {
            //Debug.Log("combo Window closed");
            comboWindowOpen = false;
        }

        public void OnCollisionStay(Collision collision)
        {
            if (currentState == PlayerState.Roll)
            {
                GameObject other = collision.gameObject;

                if (other.CompareTag("Destroyable"))
                {
                    //Debug.Log("Player hit Destroyable");
                    HealthManager otherHealth = other.GetComponent<HealthManager>();

                    if (otherHealth != null)
                    {
                        otherHealth.TakeDamage(1000);
                    }
                }
            }
        }

        //private void LateUpdate()
        //{
        //    if (gameInputSO.InteractPressed)
        //    {
        //        GameManager.Instance.gameInput.ResetUseItemPressed();
        //    }
        //}

        public void PlayWeaponVFX()
        {
            myWeaponAttributes.PlayVFX();
        }
    }
}

