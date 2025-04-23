using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.Shop;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfDesseno : BaseMask
    {
        public int minAmountOfCoinsPerDamage = 5;
        public int maxAmountOfCoinsPerDamage = 23;
        public float extraCoinsPercentage = 25;
        public int minAmountOfCoinsPerParry = 11;
        public int maxAmountOfCoinsPerParry = 38;
        public float extraDamagePerCoinPercent = 0.23f;
        public float looseCoinsWhenDamagedPercent = 2f;
        public float extraDamageReceivePerCoin = 1.4f;

        PlayerInventory myPlayerInventory;

        public int numberOfReducedItems = 2;
        public int boughtItemsToReset = 4;
        int boughtItemsCounter;

        public override void Equip()
        {
            myPlayerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
            CustomEvents.OnDamageDelt += OnDamageDelt;
            CustomEvents.OnDamageReceived += OnDamageReceived;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
        }

        private void OnCoinsCollected(int amount)
        {
            myPlayerInventory.currencyAmount += (int)(amount * extraCoinsPercentage / 100f);
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                int rand = Random.Range(minAmountOfCoinsPerParry, maxAmountOfCoinsPerParry + 1);
                myPlayerInventory.currencyAmount += rand;
            }
        }

        private void OnDamageReceived(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            myPlayerInventory.currencyAmount -= (int)(myPlayerInventory.currencyAmount * looseCoinsWhenDamagedPercent / 100f);

            int value = (int)(myPlayerInventory.currencyAmount * extraDamageReceivePerCoin / 100f);
            healthmanager.TakeDamage(value);
        }

        private void OnDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int rand = Random.Range(minAmountOfCoinsPerDamage, maxAmountOfCoinsPerDamage + 1);
            myPlayerInventory.currencyAmount += rand;

            int value = (int)(myPlayerInventory.currencyAmount * extraDamagePerCoinPercent / 100f);
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

            if (boughtItemsCounter >= boughtItemsToReset)
            {
                currentAbilityCooldown = 0;
                abilityAvailable = true;
                CustomEvents.OnShopOpen -= OnShopOpen;
                CustomEvents.OnItemPurchase -= OnItemPurchase;
            }
        }
    }
}





//Mask of Dosseno: [insert Mask description]
//[insert Mask image][insert Mask lore]

//Effects:
//Damaging an enemy makes the player gain a random amount of gold (5-23).
//the player gains +25% gold.
//Successfully parrying an opposite scale attack grants a random amount of money (11-38)
//+Major/Minor scale damage by 0.23% per total held currency
//Player will now lose 2% of total held currency on getting damaged
//Player will now take +1.4% damage based on total held currency

//special ability:
//On activation, the next two store items you will buy are discounted by 15% (buy 4 items to reset cooldown).
