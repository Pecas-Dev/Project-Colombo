using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectColombo.Objects.Masks
{
    public abstract class BaseAbility : MonoBehaviour
    {
        public Sprite abilitySprite;
        public string abilityName;
        [TextArea] public string abilityDescription;

        public float cooldownInSeconds;
        [ReadOnlyInspector] public float timer = 0;
        [ReadOnlyInspector] public bool available = true;

        public float abilityDuration;
        [ReadOnlyInspector] public float abilityTimer = 0;
        [ReadOnlyInspector] public bool active;

        [ReadOnlyInspector] public bool wasNotAvailable = false;

        public string abilitySoundName;


        [HideInInspector] public PlayerStateMachine myPlayerStateMachine;

        private void Start()
        {
            SceneManager.activeSceneChanged += SetStateMachine;
            timer = cooldownInSeconds;
        }

        private void SetStateMachine(Scene arg0, Scene arg1)
        {
            GameObject player = GameObject.Find("Player");

            if (player != null)
            {
                myPlayerStateMachine = player.GetComponent<PlayerStateMachine>();
            }
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SetStateMachine;
        }

        public bool Activate()
        {
            if (available)
            {
                timer = 0;
                
                GameObject player = GameObject.Find("Player");

                if (player != null)
                {
                    myPlayerStateMachine = player.GetComponent<PlayerStateMachine>();
                }
                else
                {
                    Debug.Log("player not found in base ability");
                    return false;
                }

                Debug.Log("ability used");
                available = false;
                active = true;
                UseAbility();
                return true;
            }

            Debug.Log("ability is not ready");
            return false;
        }

        private void Update()
        {
            if (timer < cooldownInSeconds)
            {
                available = false;
                wasNotAvailable = true;
                timer += Time.deltaTime;
            }

            if (timer >= cooldownInSeconds)
            {
                if (!available && wasNotAvailable)
                {
                    available = true;
                    wasNotAvailable = false;

                    CustomEvents.MaskAbilityReady();
                    Debug.Log($"Mask ability '{abilityName}' is ready again - cooldown complete");
                }
                else if (!available)
                {
                    available = true;
                }
            }

            if (active)
            {
                abilityTimer += Time.deltaTime;

                if (abilityTimer > abilityDuration)
                {
                    EndAbility();
                    abilityTimer = 0;
                    active = false;
                }
            }
        }
        public abstract void UseAbility();
        public abstract void EndAbility();
    }
}