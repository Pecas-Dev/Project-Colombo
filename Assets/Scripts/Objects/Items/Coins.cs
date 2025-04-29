using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Items
{
    public class Coins : MonoBehaviour
    {
        [HideInInspector] public int amount;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                CustomEvents.CoinsCollected(amount);
                GameManager.Instance.GetComponent<PlayerInventory>().currencyAmount += amount;
                Destroy(this.gameObject);
            }
        }
    }
}