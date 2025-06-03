using ProjectColombo.Camera;
using ProjectColombo.Combat;
using ProjectColombo.Enemies;
using ProjectColombo.Enemies.Pathfinding;
using ProjectColombo.Enemies.Pulcinella;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.VFX;
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
        public SkinnedMeshRenderer mySkin;

        public PulcinellaState currentStateEnum;
        [ReadOnlyInspector] public Transform playerRef;
        [ReadOnlyInspector] public bool extraDamage = false;

        [ReadOnlyInspector] public int lastAttack = -1;
        [ReadOnlyInspector] public int consecutiveAttackCount = 0;

        [ReadOnlyInspector] public bool inAir = false;

        public GameObject impactSphere;
        public GameObject cursedNoteSphere;

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
            myEntityAttributes.SetScale(GameGlobals.MusicScale.MAJOR);

            StartCoroutine(SwitchSkinColor());
            GetComponent<PulcinellaVFX>().PlaySmokeEffect();

            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            CustomEvents.OnDamageDelt += OnDamageDelt;

            myPathfindingAlgorythm.gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, HealthManager healthmanager, int combolength)
        {
            if (!extraDamage) return;

            healthmanager.TakeDamage((int)((myPulcinellaAttributes.damageMultiplier - 1f) * amount));
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            StartCoroutine(ApplyExtraDamage());
        }

        IEnumerator ApplyExtraDamage()
        {
            extraDamage = true;

            yield return new WaitForSeconds(myPulcinellaAttributes.extraDamageDuration);

            extraDamage = false;
        }

        private void FixedUpdate()
        {
            if (myHealthManager.CurrentHealth <= 0 && currentStateEnum != PulcinellaState.DEAD)
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
            currentStateEnum = newState;
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
            DisableLeftHand();
            SwitchState(new PulcinellaStateIdle(this));
        }

        public void EnableLeftHand()
        {
            leftHand.EnableWeaponHitbox();
        }

        public void DisableLeftHand()
        {
            leftHand.DisableWeaponHitbox();
        }

        public void DoSlash()
        {
            inAir = false;
            SwitchState(new PulcinellaStateAttack(this, 0));
        }

        public void SpawnRageFullImpactSphere()
        {
            Vector3 pos = transform.position + transform.forward * 0.5f;
            pos.y = 0;
            Instantiate(impactSphere, pos, transform.rotation);
        }

        public void StartMovingOnLeap()
        {
            inAir = true;
        }

        public void StopMovingOnLeap()
        {
            inAir = false;
        }

        public void SpawnCursedNoteSphere()
        {
            Vector3 pos = transform.position + transform.forward * 0.5f;
            pos.y = 0;
            GameObject sphere = Instantiate(cursedNoteSphere, pos, transform.rotation);
            sphere.GetComponent<CursedNoteWave>().SetVFX(myEntityAttributes.currentScale);
        }

        public void ImpactFeedback()
        {
            Rumble(1.0f, 0.5f, 0.5f);
            ScreenShake();
        }

        public void SwitchScale()
        {
            if (myEntityAttributes.currentScale == GameGlobals.MusicScale.MAJOR)
            {
                myEntityAttributes.SetScale(GameGlobals.MusicScale.MINOR);
            }
            else if (myEntityAttributes.currentScale == GameGlobals.MusicScale.MINOR)
            {
                myEntityAttributes.SetScale(GameGlobals.MusicScale.MAJOR);
            }

            StartCoroutine(SwitchSkinColor());
            GetComponent<PulcinellaVFX>().PlaySmokeEffect();
        }

        IEnumerator SwitchSkinColor()
        {
            mySkin.material.SetFloat("_Blend", 1f);

            yield return new WaitForSeconds(0.5f);

            mySkin.material.SetFloat("_Blend", 0f);
            mySkin.material.SetFloat("_Maj_Min", myEntityAttributes.currentScale == GameGlobals.MusicScale.MINOR ? 1f : 0f);
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

        private void OnDestroy()
        {

            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}