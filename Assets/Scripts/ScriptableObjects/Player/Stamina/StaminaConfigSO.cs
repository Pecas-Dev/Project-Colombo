using UnityEngine;


namespace ProjectColombo.Combat
{
    [CreateAssetMenu(fileName = "PlayerStamina", menuName = "Scriptable Objects/Combat/StaminaConfiguration")]
    public class StaminaConfigSO : ScriptableObject
    {
        [Header("Stamina Settings")]
        [SerializeField, Range(1, 10)] float maxStaminaPoints = 5f;
        [SerializeField, Range(0.1f, 5f)] float staminaRegenerationDelay = 1f;
        [SerializeField, Range(0.1f, 5f)] float staminaRegenerationRate = 1f;
        [SerializeField, Range(0.1f, 5f)] float staminaPointVisualFillDuration = 0.5f;

        [Header("Action Costs")]
        [SerializeField, Range(0, 5)] float rollStaminaCost = 1f;
        [SerializeField, Range(0, 5)] float comboStaminaCost = 1f;

        public float MaxStaminaPoints => maxStaminaPoints;
        public float StaminaRegenerationDelay => staminaRegenerationDelay;
        public float StaminaRegenerationRate => staminaRegenerationRate;
        public float StaminaPointVisualFillDuration => staminaPointVisualFillDuration;
        public float RollStaminaCost => rollStaminaCost;
        public float ComboStaminaCost => comboStaminaCost;
    }
}