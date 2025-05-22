using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.StateMachine.Player;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class DoubleStaminaPercentageLuck : BaseAttributes
    {
        public float chanceForDoubleStamina;
        public float chanceReductionPerLuck;

        PlayerStateMachine myPlayerStateMachine;

        public override void Enable()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();
            CustomEvents.OnStaminaUsed += OnStaminaUsed;
        }

        private void OnStaminaUsed()
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int rand = Random.Range(0, 101);
            chanceForDoubleStamina = chanceForDoubleStamina - (chanceReductionPerLuck * myPlayerStateMachine.myPlayerInventory.currentLuck);

            if (rand < chanceForDoubleStamina)
            {
                if (myPlayerStateMachine.currentComboString.Length > 2 || myPlayerStateMachine.currentState == PlayerStateMachine.PlayerState.Roll)
                {
                    Debug.Log("used extra stamina");
                    myPlayerStateMachine.myStamina.TryConsumeStamina(1);
                }
                else
                {
                    Debug.Log("not last combo or roll");
                }
            }
            else
            {
                Debug.Log("missed double stamina chance");
            }
        }

        public override void Disable()
        {
            CustomEvents.OnStaminaUsed -= OnStaminaUsed;
        }
    }
}