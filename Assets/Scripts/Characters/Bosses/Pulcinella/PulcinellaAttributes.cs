using UnityEngine;

namespace ProjectColombo.Enemies.Pulcinella
{
    public class PulcinellaAttributes : MonoBehaviour
    {
        [Header("General")]
        public float targetDistance;
        public float idleTimeAfterAttack;

        public float extraDamageDuration;
        public float damageMultiplier;

        [Header("Attacks")]

        public float minTimeBetweenAttacks;
        public float maxTimeBetweenAttacks;

        public float distanceToSlash;
        public float distanceToRageImpact;
        public int chanceToLeap;
    }
}