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
            value = myStamina.regenTime/ 100f * staminaRegenIncrease;
            Debug.Log("increased stamina regen speed from: " + myStamina.regenTime + ", by: " + staminaRegenIncrease);
            myStamina.regenTime += value;
        }


        public override void Remove()
        {
            myStamina.regenTime -= value;
        }
    }
}