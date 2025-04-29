using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using System.Data;
using UnityEngine;

namespace ProjectColombo.Inventory.Collectable
{
    public class BaseCollectable : MonoBehaviour
    {
        public CollectableData myData;
        PlayerInventory playerInventory;

        private void Start()
        {
            GetComponent<Renderer>().material.color = myData.itemColor;
            playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && myData.isPickable)
            {
                OnCollected(other.gameObject);
            }
        }

        public void OnCollected(GameObject player)
        {
            switch (myData.type)
            {
                case CollectableData.CollectibleType.Currency:
                    int amount = Random.Range(myData.minAmount, myData.maxAmount);
                    CustomEvents.CoinsCollected(amount);
                    break;

                case CollectableData.CollectibleType.Charm:

                    if (myData.charmType == CollectableData.CharmType.Health)
                    {
                        HealthCharm(player);
                    }
                    else if (myData.charmType == CollectableData.CharmType.Attack)
                    {
                        AttackCharm(player);
                    }
                    else
                    {
                        Debug.Log("charm did not work");
                    }

                    break;

                case CollectableData.CollectibleType.Mask:
                    break;

                default:
                    Debug.Log("no behaviour defined");
                    break;
            }

            //add vfx
            Destroy(this.gameObject);
        }

        private void HealthCharm(GameObject player)
        {
            player.GetComponent<HealthManager>().AddHealthPercentage(myData.valueModifierInPercent);
        }

        private void AttackCharm(GameObject player)
        {
        }

        private void Reset()
        {
            if (!TryGetComponent<Collider>(out _))
            {
                gameObject.AddComponent<BoxCollider>();
            }

            GetComponent<Collider>().isTrigger = true;

            if (!TryGetComponent<MeshRenderer>(out _))
            {
                gameObject.AddComponent<MeshRenderer>();
            }
        }
    }
}