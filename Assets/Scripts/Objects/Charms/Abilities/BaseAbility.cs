using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Net.NetworkInformation;
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
            timer = 0;

            if (available)
            {
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
                UseAbility();
                available = false;
                active = true;
                return true;
            }

            Debug.Log("ability is not ready");
            return false;
        }

        private void Update()
        {
            if (timer < cooldownInSeconds)
            {
                timer += Time.deltaTime;
            }

            if (timer >= cooldownInSeconds)
            {
                available = true;
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