using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class DamageDelt : BaseEchoMissions
    {
        public int damageToDeal;
        int currentCollected = 0;
        bool isEnabled = false;

        public override void Enable()
        {
            isEnabled = true;
            CustomEvents.OnDamageDelt += OnDamageDelt;
            Debug.Log($"<color=#FF00FF>[DamageDelt] Event listener ENABLED. Current: {currentCollected}/{damageToDeal}</color>");
        }

        private void OnDamageDelt(int value, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            if (!isEnabled)
            {
                Debug.LogError("<color=#FF00FF>[DamageDelt] Received event but handler is DISABLED!</color>");
                return;
            }

            Debug.Log($"<color=#FF00FF>[DamageDelt] Damage received: {value}, Current: {currentCollected} -> {currentCollected + value}/{damageToDeal}</color>");
            currentCollected += value;

            if (currentCollected >= damageToDeal)
            {
                Debug.Log($"<color=#FF00FF>[DamageDelt] Mission COMPLETED! ({currentCollected}/{damageToDeal})</color>");
                CompletedMission();
            }
        }

        public override void Disable()
        {
            isEnabled = false;
            CustomEvents.OnDamageDelt -= OnDamageDelt;
            Debug.Log($"<color=#FF00FF>[DamageDelt] Event listener DISABLED. Current: {currentCollected}/{damageToDeal}</color>");
        }

        public override void ResetProgress()
        {
            currentCollected = 0;
            Debug.Log($"<color=#FF00FF>[DamageDelt] Progress RESET to 0/{damageToDeal}</color>");
        }

        public override void ResetProgress()
        {
            currentCollected = 0;
        }
    }
}