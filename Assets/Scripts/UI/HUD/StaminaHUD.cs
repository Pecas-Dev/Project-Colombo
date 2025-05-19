using ProjectColombo.Combat;
using UnityEngine;
using TMPro;
using System.Collections.Generic;


namespace ProjectColombo.UI.HUD
{
    public class StaminaHUD : MonoBehaviour
    {
        public Stamina myStamina;
        public TMP_Text staminaText;
        public GameObject firstIndicator;
        public float offsetX = 90;


        List<GameObject> indicators = new();

        void Awake()
        {
            myStamina = FindFirstObjectByType<Stamina>();
        }

        private void Update()
        {
            UpdateVisuals();
            UpdateText();
        }

        public void Reset()
        {
            foreach (GameObject o in indicators)
            {
                Destroy(o);
            }

            firstIndicator.SetActive(true);

            indicators = new List<GameObject>();

            //Debug.Log("max Stamina: " + myStamina.maxStamina);

            RectTransform firstRT = firstIndicator.GetComponent<RectTransform>();
            Vector2 firstPos = firstRT.anchoredPosition;

            for (int i = 0; i < myStamina.currentMaxStamina; i++)
            {
                // Clone the indicator
                GameObject newIndicator = Instantiate(firstIndicator, firstIndicator.transform.parent);

                // Offset position
                RectTransform newRT = newIndicator.GetComponent<RectTransform>();
                newRT.anchoredPosition = new Vector2(firstPos.x + (offsetX * i), firstPos.y);

                indicators.Add(newIndicator);
            }

            firstIndicator.SetActive(false);

            UpdateText();
            UpdateVisuals();
        }

        void UpdateText()
        {
            staminaText.text = Mathf.FloorToInt(myStamina.currentStamina) + " / " + myStamina.currentMaxStamina;
        }

        void UpdateVisuals()
        {
            float current = myStamina.currentStamina;

            if (indicators.Count == 0) return;
            
            for (int i = 0; i < indicators.Count; i++)
            {
                if (current >= i + 1)
                {
                    indicators[i].GetComponentInChildren<StaminaIndicator>().UpdateDisplay(1);
                }
                else if (current > i)
                {
                    indicators[i].GetComponentInChildren<StaminaIndicator>().UpdateDisplay(current % 1);
                }
                else
                {
                    indicators[i].GetComponentInChildren<StaminaIndicator>().UpdateDisplay(0);
                }
            }
        }
    }
}