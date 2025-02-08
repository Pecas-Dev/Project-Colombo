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
        public Image maxHealthImage;
        public Image currentHealthImage;

        int maxHealth = 100;
        int currentHealth = 50;

        void Start()
        {
            playerHealthManager = GetComponentInParent<HealthManager>();
        }

        // Update is called once per frame
        void Update()
        {
            maxHealth = playerHealthManager.MaxHealth;
            currentHealth = playerHealthManager.CurrentHealth;

            healthText.text = currentHealth + " / " + maxHealth;
            maxHealthImage.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * 5 + 10, 50);
            currentHealthImage.GetComponent<RectTransform>().sizeDelta = new Vector2(currentHealth * 5, 40);
        }
    }
}