using ProjectColombo.GameManagement.Events;
using ProjectColombo.Objects.Charms;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Combat.ComboMeter
{
    public class ComboMeter : MonoBehaviour
    {
        [ReadOnlyInspector] public int currentLevel = 0;
        public int maxLevel = 3;
        [ReadOnlyInspector] public int currentPoints = 0;
        public int pointsForNextLevel = 100;

        [Header("Increase Meter")]
        public int pointsPerDamageOppositeScale;
        public int pointsPerDamageSameScale;
        public int pointsPerSuccessfullParry;
        public int pointsPerKill;

        [Header("Decrease Meter")]
        public int loosePointsPerReceiveDamageOne;
        public int loosePointsPerReceiveDamageTwo;
        public int loosePointsPerReceiveDamageThree;

        public int loosePointsPerFailedOppParryOne;
        public int loosePointsPerFailedOppParryTwo;
        public int loosePointsPerFailedOppParryThree;

        public GameObject attribLevelOne;
        public GameObject attribLevelTwo;
        public GameObject attribLevelThree;

        private void Start()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            CustomEvents.OnEnemyDeath += OnEnemyDeath;
            CustomEvents.OnDamageReceived += OnDamageReceived;
            CustomEvents.OnParryFailed += OnParryFailed;
        }

        private void OnEnemyDeath(GameGlobals.MusicScale scale, GameObject enemy)
        {
            if (currentLevel < maxLevel || currentPoints < pointsForNextLevel)
            {
                AddPoints(pointsPerKill);
            }
        }

        private void OnDamageDelt(int damage, GameGlobals.MusicScale scale, bool sameScale, HealthManager healthmanager, int comboLength)
        {
            if (currentLevel < maxLevel || currentPoints < pointsForNextLevel)
            {
                if (sameScale) AddPoints(pointsPerDamageSameScale);
                else AddPoints(pointsPerDamageOppositeScale);
            }
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (currentLevel < maxLevel || currentPoints < pointsForNextLevel)
            {
                AddPoints(pointsPerSuccessfullParry);
            }
        }

        private void OnParryFailed(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager, bool sameScale)
        {
            if (!sameScale)
            {            
                if (currentLevel > 0 || currentPoints > 0)
                {
                    if (currentLevel == 3) AddPoints(-loosePointsPerFailedOppParryThree);
                    else if (currentLevel == 2) AddPoints(-loosePointsPerFailedOppParryTwo);
                    else AddPoints(-loosePointsPerFailedOppParryOne);
                }
            }
            else
            {
                OnDamageReceived(damage, scale, healthmanager);
            }
        }

        private void OnDamageReceived(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            if (currentLevel > 0 || currentPoints > 0)
            {
                if (currentLevel == 3) AddPoints(-loosePointsPerReceiveDamageThree);
                else if (currentLevel == 2) AddPoints(-loosePointsPerReceiveDamageTwo);
                else AddPoints(-loosePointsPerReceiveDamageOne);
            }
        }

        void AddPoints(int amount)
        {
            currentPoints += amount;

            if (currentPoints >= pointsForNextLevel)
            {
                IncreaseLevel();
                currentPoints -= pointsForNextLevel;
            }
            else if (currentPoints < 0)
            {
                DecreaseLevel();
                currentPoints += pointsForNextLevel;
            }

            currentPoints = Mathf.Clamp(currentPoints, 0, pointsForNextLevel);
        }

        void IncreaseLevel()
        {
            currentLevel++;
            currentLevel = Mathf.Min(currentLevel, maxLevel);
            CustomEvents.ComboMeterLevelIncreased(currentLevel);

            DeactivateAllAttribStorages();
            ActivateAtrribStorage(currentLevel);
        }

        void DecreaseLevel()
        {
            currentLevel--;
            currentLevel = Mathf.Max(currentLevel, 0);
            CustomEvents.ComboMeterLevelDecreased(currentLevel);

            DeactivateAllAttribStorages();
            ActivateAtrribStorage(currentLevel);
        }

        void ActivateAtrribStorage(int currentLevel)
        {
            if (currentLevel == 0) return;

            GameObject attribStorage = attribLevelOne;

            if (currentLevel == 2) attribStorage = attribLevelTwo;
            else if (currentLevel == 3) attribStorage = attribLevelThree;

            BaseAttributes[] attribs = attribStorage.GetComponentsInChildren<BaseAttributes>();

            foreach (var attrib in attribs)
            {
                attrib.Enable();
            }
        }

        void DeactivateAllAttribStorages()
        {
            var allAttribs = new System.Collections.Generic.List<BaseAttributes>();

            allAttribs.AddRange(attribLevelOne.GetComponentsInChildren<BaseAttributes>());
            allAttribs.AddRange(attribLevelTwo.GetComponentsInChildren<BaseAttributes>());
            allAttribs.AddRange(attribLevelThree.GetComponentsInChildren<BaseAttributes>());

            foreach (var attrib in allAttribs)
            {
                attrib.Disable();
            }
        }
    }
}