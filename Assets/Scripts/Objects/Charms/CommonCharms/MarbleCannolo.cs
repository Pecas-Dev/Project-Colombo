using ProjectColombo.Combat;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MarbleCannolo : BaseCharm
    {
        public int staminaRegenIncrease = 10;
        float value;
        Stamina myStamina;

        public override void Equip()
        {
            myStamina = GameObject.Find("Player").GetComponent<Stamina>();
            value = myStamina.regenSpeed/ 100f * staminaRegenIncrease;
            myStamina.regenSpeed += value;
        }


        public override void Remove()
        {
            myStamina.regenSpeed -= value;
        }
    }
}