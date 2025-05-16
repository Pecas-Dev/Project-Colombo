using ProjectColombo.Combat;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ProjectColombo.UI.HUD
{
    public class Healthbar : MonoBehaviour
    {
        HealthManager playerHealthManager;
        public TMP_Text healthText;
        //public Image maxHealthImage;
        public Image currentHealthImage;

        int maxHealth = 0;
        int currentHealth = 0;

        void Start()
        {
            playerHealthManager = GetComponentInParent<HealthManager>();
        }

        void Update()
        {
            maxHealth = playerHealthManager.MaxHealth;
            currentHealth = playerHealthManager.CurrentHealth;

            healthText.text = currentHealth + "\n -- \n" + maxHealth;
            //maxHealthImage.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * 0.5f + 10, 50);
            currentHealthImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}