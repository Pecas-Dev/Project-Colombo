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


        public SkinnedMeshRenderer myFadeOutSkin;
        public MeshRenderer myMajorMinorSkin;

        public MommottiState currentState;
        GameGlobals.MusicScale hitByScale;

        //set speed for animator
        Vector3 positionLastFrame;

        private void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myMommottiAttributes = GetComponent<MommottiAttributes>();
            myPathfindingAlgorythm = GetComponent<Pathfinding>();
            myWeaponAttributes = GetComponentInChildren<WeaponAttributes>();
            myHealthManager = GetComponent<HealthManager>();
            SetScale(GameGlobals.MusicScale.MINOR);
        }

        void Start()
        {
            LogMissingReferenceErrors();

            SwitchState(new MommottiStatePatrol(this));
            positionLastFrame = transform.position;
            myPathfindingAlgorythm.gridManager = myMommottiAttributes.myGridManager;
        }

        private float smoothedSpeed = 0f; // Store smoothed speed
        public float speedSmoothTime = 0.1f; // Adjustable in inspector

        private void FixedUpdate()
        {
            if (myHealthManager.CurrentHealth <= 0 && currentState != MommottiState.DEAD)
            {
                SwitchState(new MommottiStateDeath(this, hitByScale));
            }

            // Calculate raw speed
            Vector3 movementLastFrame = positionLastFrame - transform.position;
            movementLastFrame.y = 0;
            float currentSpeed = movementLastFrame.magnitude / Time.fixedDeltaTime;

            // Smooth the speed
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, speedSmoothTime / Time.fixedDeltaTime);

            // Update animator
            myAnimator.SetFloat("Speed", smoothedSpeed > 0.1f ? smoothedSpeed : 0f);

            // Update position
            positionLastFrame = transform.position;

            if (myMommottiAttributes.playerPosition.gameObject.GetComponent<HealthManager>().CurrentHealth <= 0 && currentState != MommottiState.PATROL)
            {
                SwitchState(new MommottiStatePatrol(this));
            }
        }


        public void ApplyKnockback(Vector3 direction, float knockbackStrength, GameGlobals.MusicScale scale)
        {
            if (myHealthManager.CurrentHealth <= 0)
            {
                knockbackStrength *= 1.5f;
            }

            hitByScale = scale;
            myRigidbody.AddForce(direction * knockbackStrength, ForceMode.Impulse);
        }

        public void SetStaggered()
        {
            InterruptAttack();
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
            myWeaponAttributes.EnableWeaponHitbox();
        }

        public void InterruptAttack()
        {
            myWeaponAttributes.isAttacking = false;
            myWeaponAttributes.DisableWeaponHitbox();
        }

        public void Telegraphing()
        {
            myWeaponAttributes.Telegraphing();
        }

        public void SetScale(GameGlobals.MusicScale scale)
        {
            myEntityAttributes.currentScale = scale;
            myWeaponAttributes.currentScale = scale;

            Color color = scale == GameGlobals.MusicScale.MAJOR ? GameGlobals.majorColor : GameGlobals.minorColor;

            myMajorMinorSkin.material.SetColor("_Major_MinorColor", color);
        }

        public void SetAttackingState()
        {
            SwitchState(new MommottiStateAttack(this));
        }

        public void SetChaseState()
        {
            SwitchState(new MommottiStateChase(this));
        }
    }
}