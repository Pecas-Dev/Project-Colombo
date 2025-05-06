using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{

    public class DamagePercentageLuck : BaseAttributes
    {
        public float damagePercentageLuck;
        public int forHowManyLuck;

        PlayerInventory myPlayerInventroy;

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myPlayerInventroy = GameManager.Instance.GetComponent<PlayerInventory>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int multiplyer = Mathf.FloorToInt(myPlayerInventroy.currentLuck / forHowManyLuck);
            int value = (int)(multiplyer * damagePercentageLuck / 100f);

            Debug.Log("dealt " + value + " extra damage for luck: " + myPlayerInventroy.currentLuck);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}