using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfDsseno : BaseMask
    {
        [Header("General Buffs")]
        public int minAmountOfCoinsPerDamage = 5;
        public int maxAmountOfCoinsPerDamage = 23;
        public float extraCoinsPercentage = 25;
        public int minAmountOfCoinsPerParry = 11;
        public int maxAmountOfCoinsPerParry = 38;
        public float extraDamagePerCoinPercent = 0.23f;
        public float looseCoinsWhenDamagedPercent = 2f;
        public float extraDamageReceivePerCoin = 1.4f;

        [Header("Echo Misson")]
        public int goldToCollect = 1500;
        int currentCollectedGold = 0;

        [Header("Upgraded Buffs after Echo")]
        public int minAmountOfCoinsPerDamageEcho = 7;
        public int maxAmountOfCoinsPerDamageEcho = 28;
        public float extraCoinsPercentageEcho = 30;
        public int minAmountOfCoinsPerParryEcho = 18;
        public int maxAmountOfCoinsPerParryEcho = 43;
        public float extraDamagePerCoinPercentEcho = 0.41f;
        public int addedGoldEcho = 100;



        [Header("Ability Stats")]
        public int numberOfReducedItems = 2;
        public int boughtItemsToReset = 4;
        int boughtItemsCounter;


        PlayerInventory myPlayerInventory;

        public override void Equip()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnDamageDelt += OnDamageDelt;
            CustomEvents.OnDamageReceived += OnDamageReceived;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
        }

        private void OnCoinsCollected(int amount)
        {
            int value = (int)(amount * extraCoinsPercentage / 100f);
            Debug.Log("extra money: " + value);
            myPlayerInventory.currencyAmount += value;

            currentCollectedGold += amount + value;

            if (currentCollectedGold > goldToCollect && !echoUnlocked)
            {
                UnlockEcho();
            }
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                int rand = Random.Range(minAmountOfCoinsPerParry, maxAmountOfCoinsPerParry + 1);
                Debug.Log("extra money for samescale successfull parry: " + rand);
                myPlayerInventory.currencyAmount += rand;
            }
        }

        private void OnDamageReceived(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int lost = (int)(myPlayerInventory.currencyAmount * looseCoinsWhenDamagedPercent / 100f);
            myPlayerInventory.currencyAmount -= lost;
            Debug.Log("coins lost: " + lost);

            int value = (int)(myPlayerInventory.currencyAmount * extraDamageReceivePerCoin / 100f);
            Debug.Log("extra damage for coins: " + value);
            healthmanager.TakeDamage(value);
        }

        private void OnDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int rand = Random.Range(minAmountOfCoinsPerDamage, maxAmountOfCoinsPerDamage + 1);
            myPlayerInventory.currencyAmount += rand;
            Debug.Log("coins gained: " + rand);

            int value = (int)(myPlayerInventory.currencyAmount * extraDamagePerCoinPercent / 100f);
            Debug.Log("extra damage delt for coins: " + value);
            healthmanager.TakeDamage(value);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
            CustomEvents.OnDamageReceived -= OnDamageReceived;
            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
            CustomEvents.OnItemPurchase -= OnItemPurchase;
            CustomEvents.OnShopOpen -= OnShopOpen;
        }

        public override void UseAbility()
        {
            currentAbilityCooldown = Mathf.Infinity;

            CustomEvents.OnShopOpen += OnShopOpen;
            CustomEvents.OnItemPurchase += OnItemPurchase;
        }

        private void OnShopOpen()
        {
            //TODO when shop ready
        }

        private void OnItemPurchase(int obj)
        {
            boughtItemsCounter++;
            Debug.Log("item purchased: " + boughtItemsCounter);

            if (boughtItemsCounter >= boughtItemsToReset)
            {
                Debug.Log("end ability");
                currentAbilityCooldown = 0;
                abilityAvailable = true;
                CustomEvents.OnShopOpen -= OnShopOpen;
                CustomEvents.OnItemPurchase -= OnItemPurchase;
            }
        }

        public override void UnlockEcho()
        {
            Remove();

            echoUnlocked = true;

            minAmountOfCoinsPerDamage = minAmountOfCoinsPerDamageEcho;
            maxAmountOfCoinsPerDamage = maxAmountOfCoinsPerDamageEcho;
            extraCoinsPercentage = extraCoinsPercentageEcho;
            minAmountOfCoinsPerParry = minAmountOfCoinsPerParryEcho;
            maxAmountOfCoinsPerParry = maxAmountOfCoinsPerParryEcho;
            extraDamagePerCoinPercent = extraDamagePerCoinPercentEcho;
            myPlayerInventory.currencyAmount += addedGoldEcho;

            Equip();
        }
    }
}





//Effects:
//Damaging an enemy makes the player gain a random amount of gold (3-21).
//Killing an enemy increases the gold the player gains +25% gold.
//Successfully parrying an opposite scale attack grants a random amount of money (11-38)
//+Major/Minor scale damage by 0.23% per total held currency
//Player will now lose 2% of total held currency on getting damaged
//Player will now take +1.4% damage based on total held currency
//+200 gold on pickup/selection


//ECHO OF THE MASK:
//Collect 1500 gold


//Awakened Stats:
//Damaging an enemy makes the player gain a random amount of gold (7-28).
//Killing an enemy increases the gold the player gains +30% gold.
//Successfully parrying an opposite scale attack grants a random amount of money (18-43)
//+Major/Minor scale damage by 0.41% per total held currency
//+100 gold (just adds 100 gold)

//Special Ability: Greed of Knowledge
//On activation, the next two store items you will buy are discounted by 15% (buy 4 items to reset cooldown).
