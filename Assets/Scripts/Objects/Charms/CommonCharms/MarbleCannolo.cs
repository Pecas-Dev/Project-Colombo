using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MarbleCannolo : BaseCharm
    {
        public int staminaRegenIncrease = 10;
        float defaultSpeed;

        public override void Equip()
        {
            defaultSpeed = GetComponentInParent<Stamina>().regenSpeed;
            GetComponentInParent<Stamina>().regenSpeed += defaultSpeed / 100f * staminaRegenIncrease;
        }


        public override void Remove()
        {
            GetComponentInParent<Stamina>().regenSpeed = defaultSpeed;
        }
    }
}