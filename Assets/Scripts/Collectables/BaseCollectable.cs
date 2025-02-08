using ProjectColombo.Combat;
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
            playerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
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
                    playerInventory.AddCurrency(Random.Range(myData.minAmount, myData.maxAmount));
                    break;

                case CollectableData.CollectibleType.Charm:
                    playerInventory.AddCharm(myData.charmType);

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
                    playerInventory.AddMask(myData.maskType);
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
            Debug.Log("Add health in base collectable");
            player.GetComponent<HealthManager>().AddHealthPercentage(myData.valueModifierInPercent);
        }

        private void AttackCharm(GameObject player)
        {
            //please change to weapon attributes....
            player.GetComponent<HitboxHandManager>().AddDamagePercentage(myData.valueModifierInPercent);
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