
using ProjectColombo.GameManagement;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public abstract class BaseMask : MonoBehaviour
    {
        public string maskName;
        public Sprite maskPicture;
        [TextArea] public string echoDescription;
        [TextArea] public string maskDescription;
        [TextArea] public string maskLore;

        [HideInInspector] public bool echoUnlocked = false;
        [HideInInspector] public bool abilityAvailable = false;
        [HideInInspector] public float currentAbilityCooldown = 0;
        [HideInInspector] public float timer = 0;

        private void Start()
        {
            GameManager.Instance.gameInput.EnableInput(GameInputSystem.InputActionType.UseSpecialAbility);
        }

        private void Update()
        {
            if (!abilityAvailable)
            {
                timer += Time.deltaTime;

                if (timer >= currentAbilityCooldown)
                {
                    Debug.Log("ability available");
                    abilityAvailable = true;
                    timer = 0;
                }
            }

            if (GameManager.Instance.gameInput.UseSpecialAbilityPressed && abilityAvailable && echoUnlocked)
            {
                Debug.Log("start ability");
                GameManager.Instance.gameInput.ResetUseSpecialAbilityPressed();
                abilityAvailable = false;
                UseAbility();
            }
        }

        public abstract void Equip();
        public abstract void UseAbility();
        public abstract void Remove();
    }
}