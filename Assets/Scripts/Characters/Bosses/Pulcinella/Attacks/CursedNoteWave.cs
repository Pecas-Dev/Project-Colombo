using ProjectColombo;
using ProjectColombo.Camera;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace ProjectColombo.Combat
{
    public class CursedNoteWave : MonoBehaviour
    {
        public int damage;
        float timer;
        public Collider myCollider;
        bool hit = false;
        bool isSlowMo = false;

        public VisualEffect majorVFX;
        public VisualEffect minorVFX;

        public float growthDuration = 7f;
        public float minSize = 1f;
        public float maxSize = 50f;

        private void Start()
        {
            myCollider.enabled = true;
        }

        void Update()
        {
            if (!hit) myCollider.enabled = true;

            timer += Time.deltaTime;
            if (timer >= growthDuration + 0.2f)
            {
                Destroy(this.gameObject);
            }

            // Calculate size growth using smoothstep for deceleration
            float t = Mathf.Clamp01(timer / growthDuration);
            float easedT = 1 - Mathf.Pow(1 - t, 2); // quadratic ease-out
            float currentSize = Mathf.Lerp(minSize, maxSize, easedT);

            // Apply size to VFX
            majorVFX.SetFloat("Size", currentSize);
            minorVFX.SetFloat("Size", currentSize);
            ((SphereCollider)myCollider).radius = currentSize * 2;
        }

        public void SetVFX(GameGlobals.MusicScale scale)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                majorVFX.Play();
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                minorVFX.Play();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStateMachine sm = other.GetComponent<PlayerStateMachine>();

                hit = true;
                myCollider.enabled = false;

                if (sm.isParrying || sm.currentStateEnum == PlayerStateMachine.PlayerState.Roll || sm.isInvunerable)
                {
                    return;
                }

                ScreenShake();
                StopTime();

                if (sm.isBlocking)
                {
                    sm.myHealthManager.TakeDamage((int)(damage * 0.6f));
                    Rumble(0.1f, 0.5f, 0.2f); // Light buzz
                }
                else
                {
                    sm.myHealthManager.TakeDamage(damage);
                    sm.SetStaggered();
                    Rumble(1.0f, 0.5f, 0.5f); // Big buzz
                }

            }
        }

        private void ScreenShake()
        {
            FindFirstObjectByType<ScreenShakeManager>().Shake(0.4f);
        }


        public void StopTime()
        {
            StartCoroutine(DoHitStop());
        }

        IEnumerator DoHitStop()
        {
            float pauseDuration = 0.2f; // Adjust the duration of the freeze

            isSlowMo = true;
            Time.timeScale = 0.1f; // Slow down time instead of freezing completely
            yield return new WaitForSecondsRealtime(pauseDuration);

            isSlowMo = false;
            Time.timeScale = 1f; // Resume normal time
        }

        public void Rumble(float big, float small, float duration)
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

        void OnDisable()
        {
            if (isSlowMo)
                Time.timeScale = 1f; // Ensure game doesn't stay stuck in slow motion

            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            gamepad.SetMotorSpeeds(0f, 0f);
        }


        private void OnDestroy()
        {
            if (isSlowMo)
                Time.timeScale = 1f;

            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
}