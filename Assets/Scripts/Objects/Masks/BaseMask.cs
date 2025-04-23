
using ProjectColombo.GameManagement;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public abstract class BaseMask : MonoBehaviour
    {
        public string maskName;
        public Texture2D maskPicture;
        [TextArea] public string maskDescription;
        [TextArea] public string maskLore;
        [HideInInspector] public bool abilityAvailable = false;
        [HideInInspector] public float currentAbilityCooldown = 0;
        [HideInInspector] public float timer = 0;

        private void Update()
        {
            if (!abilityAvailable)
            {
                timer += Time.deltaTime;

                if (timer >= currentAbilityCooldown)
                {
                    abilityAvailable = true;
                    timer = 0;
                }
            }

            if (GameManager.Instance.gameInput.UseSpecialAbilityPressed && abilityAvailable)
            {
                abilityAvailable = false;
                UseAbility();
            }
        }

        public abstract void Equip();
        public abstract void UseAbility();
        public abstract void Remove();
    }
}