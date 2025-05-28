using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class AttributesStatSheet : MonoBehaviour
    {
        //damage
        [ReadOnlyInspector] public float majorDamagePercentage = 0;
        [ReadOnlyInspector] public float minorDamagePercentage = 0;

        //defense
        [ReadOnlyInspector] public float damageResistPercentage = 0;
        [ReadOnlyInspector] public float blockPercentage = 0;
        [ReadOnlyInspector] public float extraDamageMissedParryDifferentScalePercentage = 0;
        [ReadOnlyInspector] public float extraDamageMissedParrySameScalePercentage = 0;
        [ReadOnlyInspector] public float evadeChancePercentage = 0;

        //speed
        [ReadOnlyInspector] public float attackSpeedPercentage = 0;
        [ReadOnlyInspector] public float staminaRegenSpeedPercentage = 0;
        [ReadOnlyInspector] public float moveSpeedPercentage = 0;

        //luck
        [ReadOnlyInspector] public int luckPoints = 0;

        public void ResetStats()
        {
            majorDamagePercentage = 0;
            minorDamagePercentage = 0;

            damageResistPercentage = 0;
            blockPercentage = 0;
            extraDamageMissedParryDifferentScalePercentage = 0;
            extraDamageMissedParrySameScalePercentage = 0;
            evadeChancePercentage = 0;

            attackSpeedPercentage = 0;
            staminaRegenSpeedPercentage = 0;
            moveSpeedPercentage = 0;

            luckPoints = 0;
        }
    }
}