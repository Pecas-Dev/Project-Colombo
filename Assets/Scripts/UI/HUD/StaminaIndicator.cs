using UnityEngine;
using UnityEngine.UI;


namespace ProjectColombo.UI.HUD
{
    public class StaminaIndicator : MonoBehaviour
    {
        public Image staminaDisplay;
        float targetPercentage = 1f;
        public void UpdateDisplay(float percentage)
        {
            targetPercentage = percentage; 
        }

        private void Update()
        {
            staminaDisplay.fillAmount = Mathf.Lerp(staminaDisplay.fillAmount, targetPercentage, 1f);
        }
    }
}