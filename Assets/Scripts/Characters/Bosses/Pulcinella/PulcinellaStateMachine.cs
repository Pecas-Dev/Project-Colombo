using ProjectColombo.Camera;
using ProjectColombo.Combat;
using ProjectColombo.Enemies;
using ProjectColombo.Enemies.Pathfinding;
using ProjectColombo.Enemies.Pulcinella;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


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
        public PulcinellaAttributes myPulcinellaAttributes;
        public WeaponAttributes leftHand;
        public GameObject leftHandCanvas;

        public PulcinellaState currentState;
        [ReadOnlyInspector] public Transform playerRef;

        public GameObject impactSphere;

        private void Awake()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            myEntityAttributes = GetComponent<EntityAttributes>();
            myPathfindingAlgorythm = GetComponent<Pathfinding>();
            myHealthManager = GetComponent<HealthManager>();
            myPulcinellaAttributes = GetComponent<PulcinellaAttributes>();
        }

        void Start()
        {
            LogMissingReferenceErrors();

            playerRef = GameObject.FindGameObjectWithTag("Player").transform;
            SwitchState(new PulcinellaStateIdle(this));
            leftHandCanvas.SetActive(false);

            myPathfindingAlgorythm.gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
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
                Debug.Log("Missing Rigidbody in Pulcinella!");
            }

            if (myAnimator == null)
            {
                Debug.Log("Missing Animator in Pulcinella!");
            }

            if (myEntityAttributes == null)
            {
                Debug.Log("Missing Entity Attributes in Pulcinella!");
            }

            if (myPathfindingAlgorythm == null)
            {
                Debug.Log("Missing Pathfinding Algorythm in Pulcinella!");
            }

            if (myHealthManager == null)
            {
                Debug.Log("Missing HealthManager in Pulcinella!");
            }

            if (myPulcinellaAttributes == null)
            {
                Debug.Log("Missing Pulcinella Attributes");
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

        public void OnAttackAnimationEnd()
        {
            SwitchState(new PulcinellaStateIdle(this));
        }

        public void EnableLeftHand()
        {
            leftHand.EnableWeaponHitbox();
            leftHandCanvas.SetActive(true);
        }

        public void DisableLeftHand()
        {
            leftHand.DisableWeaponHitbox();
            leftHandCanvas.SetActive(false);
        }

        public void DoSlash()
        {
            SwitchState(new PulcinellaStateAttack(this, 0));
        }

        public void SpawnRageFullImpactSphere()
        {
            Vector3 pos = new(transform.position.x, 1, transform.position.z);
            Instantiate(impactSphere, pos, transform.rotation);
        }


        public void ImpactFeedback()
        {
            Rumble(1.0f, 0.5f, 0.5f);
            ScreenShake();
        }

        void Rumble(float big, float small, float duration)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            // Clamp values between 0 and 1
            big = Mathf.Clamp01(big);
            small = Mathf.Clamp01(small);

            // Set motor speeds
            gamepad.SetMotorSpeeds(big, small);

            // Stop after duration
            StartCoroutine(StopRumbleAfter(duration));
        }

        private IEnumerator StopRumbleAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }
        }

        private void ScreenShake()
        {
            FindFirstObjectByType<ScreenShakeManager>().Shake(0.4f);
        }
    }
}