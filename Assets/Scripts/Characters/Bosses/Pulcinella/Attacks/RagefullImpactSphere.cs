using ProjectColombo.Camera;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectColombo.Combat
{
    public class RagefullImpactSphere : MonoBehaviour
    {
        public int damage;
        public float speed;
        public float timeTillDelete;
        float timer;
        public Collider myCollider;
        bool hit = false;
        bool isSlowMo = false;

        private void Start()
        {
            myCollider.enabled = true;
        }
        private void Update()
        {
            if (!hit) myCollider.enabled = true;

            timer += Time.deltaTime;

            if (timer >= timeTillDelete)
            {
                Destroy(this.gameObject);
            }

            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStateMachine sm = other.GetComponent<PlayerStateMachine>();

                hit = true;
                myCollider.enabled = false;

                ScreenShake();
                StopTime();
                sm.myHealthManager.TakeDamage(damage);
                sm.SetStaggered();
                Rumble(1.0f, 0.5f, 0.5f); // Big buzz


                Destroy(this.gameObject, 1f);
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