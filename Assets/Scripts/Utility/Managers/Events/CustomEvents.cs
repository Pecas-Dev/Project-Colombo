using UnityEngine;
using System;
using ProjectColombo.Combat;
using ProjectColombo.Shop;

namespace ProjectColombo.GameManagement.Events
{
    public class CustomEvents : MonoBehaviour
    {
        public static event Action<int, GameGlobals.MusicScale, bool, HealthManager, int> OnDamageDelt;
        public static event Action<int, GameGlobals.MusicScale, HealthManager> OnDamageReceived;
        public static event Action<GameGlobals.MusicScale, GameObject> OnEnemyDeath;
        public static event Action OnPlayerDeath;
        public static event Action<GameGlobals.MusicScale, bool> OnSuccessfullParry;
        public static event Action<int, GameGlobals.MusicScale, HealthManager, bool> OnParryFailed;
        public static event Action<int, GameGlobals.MusicScale, HealthManager> OnDamageBlocked;
        public static event Action<ShopKeeper> OnShopOpen;
        public static event Action OnShopClose;
        public static event Action<int> OnItemPurchase;
        public static event Action<int> OnCoinsCollected;
        public static event Action<GameObject> OnCharmCollected;
        public static event Action<GameObject> OnMaskCollected;
        public static event Action OnStaminaRegenerated;
        public static event Action OnStaminaUsed;
        public static event Action OnLevelChange;
        public static event Action<int> OnMaxHealthGained;
        public static event Action OnEchoUnlocked;
        public static event Action OnChamberActivated;
        public static event Action OnChamberFinished;
        public static event Action<string> OnAbilityUsed;
        public static event Action<GameObject> OnEnemyAttack;
        public static event Action<int> OnComboMeterLevelIncrease;
        public static event Action<int> OnComboMeterLevelDecrease;
        public static event Action OnPotionUsed;
        public static event Action OnCharmFirstTimeEquipped;
        public static event Action OnPlayerRoll;
        public static event Action<GameObject, int> OnDisplayDamageNumber;
        public static event Action OnBossFightStarted;
        public static event Action OnBossFightEnded;


        // PLAYSTATION LIGHT BAR COLORS 
        public static event Action<GameGlobals.MusicScale> OnMinorAttackPerformed;
        public static event Action<GameGlobals.MusicScale> OnMajorAttackPerformed;
        public static event Action<GameGlobals.MusicScale> OnMinorParryPerformed;
        public static event Action<GameGlobals.MusicScale> OnMajorParryPerformed;
        public static event Action<Color> OnLightbarColorChangeRequested;

        public static void DamageDelt(int damage, GameGlobals.MusicScale scale, bool sameScale, HealthManager enemyHealthManager, int comboLength)
        {
            OnDamageDelt?.Invoke(damage, scale, sameScale, enemyHealthManager, comboLength);
        }

        public static void DamageReceived(int damage, GameGlobals.MusicScale scale, HealthManager playerHealthManager)
        {
            OnDamageReceived?.Invoke(damage, scale, playerHealthManager);
        }

        public static void EnemyDied(GameGlobals.MusicScale scale, GameObject enemy)
        {
            OnEnemyDeath?.Invoke(scale, enemy);
        }

        public static void PlayerDied()
        {
            OnPlayerDeath?.Invoke();
        }

        public static void SuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            OnSuccessfullParry?.Invoke(scale, sameScale);
        }

        public static void FailedParry(int damage, GameGlobals.MusicScale scale, HealthManager playerHealthManager, bool sameScale)
        {
            OnParryFailed?.Invoke(damage, scale, playerHealthManager, sameScale);
        }

        public static void DamageBlocked(int damage, GameGlobals.MusicScale scale, HealthManager playerHealthManager)
        {
            OnDamageBlocked?.Invoke(damage, scale, playerHealthManager);
        }

        public static void ShopOpen(ShopKeeper shop)
        {
            OnShopOpen?.Invoke(shop);
        }

        public static void ShopClose()
        {
            OnShopClose?.Invoke();
        }

        public static void ItemPurchased(int price)
        {
            OnItemPurchase?.Invoke(price);
        }

        public static void CoinsCollected(int amount)
        {
            OnCoinsCollected?.Invoke(amount);
        }

        public static void CharmCollected(GameObject charm)
        {
            OnCharmCollected?.Invoke(charm);
        }

        public static void MaskCollected(GameObject mask)
        {
            OnMaskCollected?.Invoke(mask);
        }

        public static void StaminaRegenerated()
        {
            OnStaminaRegenerated?.Invoke();
        }

        public static void StaminaUsed()
        {
            OnStaminaUsed?.Invoke();
        }

        public static void LevelChanged()
        {
            OnLevelChange?.Invoke();
        }

        public static void MaxHealthIncreased(int amount)
        {
            OnMaxHealthGained?.Invoke(amount);
        }

        public static void EchoUnlocked()
        {
            OnEchoUnlocked?.Invoke();
        }
        public static void ChamberActivated()
        {
            OnChamberActivated?.Invoke();
        }
        public static void ChamberFinished()
        {
            OnChamberFinished?.Invoke();
        }

        public static void AbilityUsed(string audioName)
        {
            OnAbilityUsed?.Invoke(audioName);
        }

        public static void EnemyAttacked(GameObject enemy)
        {
            OnEnemyAttack?.Invoke(enemy);
        }

        public static void ComboMeterLevelIncreased(int newLevel)
        {
            OnComboMeterLevelIncrease?.Invoke(newLevel);
        }

        public static void ComboMeterLevelDecreased(int newLevel)
        {
            OnComboMeterLevelDecrease?.Invoke(newLevel);
        }

        public static void PotionUsed()
        {
            OnPotionUsed?.Invoke();
        }

        public static void CharmFirstTimeEquipped()
        {
            OnCharmFirstTimeEquipped?.Invoke();
        }

        public static void PlayerRoll()
        {
            OnPlayerRoll?.Invoke();
        }

        public static void DisplayDamageNumber(GameObject damagedEnemy, int amount)
        {
            OnDisplayDamageNumber?.Invoke(damagedEnemy, amount);
        }


        // PLAYSTATION LIGHT BAR COLORS 
        public static void MinorAttackPerformed(GameGlobals.MusicScale scale)
        {
            OnMinorAttackPerformed?.Invoke(scale);
        }

        public static void MajorAttackPerformed(GameGlobals.MusicScale scale)
        {
            OnMajorAttackPerformed?.Invoke(scale);
        }

        public static void RequestLightbarColorChange(Color newColor)
        {
            OnLightbarColorChangeRequested?.Invoke(newColor);
        }

        public static void MinorParryPerformed(GameGlobals.MusicScale scale)
        {
            OnMinorParryPerformed?.Invoke(scale);
        }

        public static void MajorParryPerformed(GameGlobals.MusicScale scale)
        {
            OnMajorParryPerformed?.Invoke(scale);
        }

        //----

        public static void StartBossfight()
        {
            OnBossFightStarted?.Invoke();
        }

        public static void EndBossfight()
        {
            OnBossFightEnded?.Invoke();
        }
    }
}