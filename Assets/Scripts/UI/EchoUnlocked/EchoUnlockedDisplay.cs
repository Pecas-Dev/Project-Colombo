using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.UI
{
    public class EchoUnlockedDisplay : MonoBehaviour
    {
        public GameObject echoUnlockedDisplay;
        public float duration;
        float timer = 0;
        bool active = false;

        private void Start()
        {
            echoUnlockedDisplay.SetActive(false);
            active = false;
            CustomEvents.OnEchoUnlocked += OnEchoUnlocked;
        }

        private void OnEchoUnlocked()
        {
            timer = 0;
            active = true;
            echoUnlockedDisplay.SetActive(true);
        }

        private void Update()
        {
            if (active)
            {
                timer += Time.deltaTime;

                if (timer >= duration)
                {
                    echoUnlockedDisplay.SetActive(false);
                }
            }
        }
    }
}