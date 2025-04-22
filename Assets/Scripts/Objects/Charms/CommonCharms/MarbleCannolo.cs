using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MarbleCannolo : BaseCharm
    {
        public int staminaRegenIncrease = 10;
        float value;

        public override void Equip()
        {
            value = GetComponentInParent<Stamina>().regenSpeed/ 100f * staminaRegenIncrease;
            GetComponentInParent<Stamina>().regenSpeed += value;
        }


        public override void Remove()
        {
            GetComponentInParent<Stamina>().regenSpeed -= value;
        }
    }
}