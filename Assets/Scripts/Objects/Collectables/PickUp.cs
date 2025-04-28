using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects.Masks;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Objects
{
    public class PickUp : MonoBehaviour
    {
        public GameObject indicator;
        public bool active = false;

        private void Start()
        {
            indicator.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                indicator.SetActive(true);
                active = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                indicator.SetActive(false);
                active = false;
            }
        }

        private void Update()
        {
            if (active && GameManager.Instance.gameInput.UseItemPressed)
            {
                GameObject charm = GetComponentInChildren<BaseCharm>().gameObject;
                GameManager.Instance.gameInput.ResetUseItemPressed();

                if (charm != null)
                {
                    CustomEvents.CharmCollected(charm);
                    Debug.Log("collected charm");
                    Destroy(this.gameObject);
                    return;
                }

                GameObject mask = GetComponentInChildren<BaseMask>().gameObject;

                if (mask != null)
                {
                    CustomEvents.MaskCollected(mask);
                    Debug.Log("collected mask");
                    Destroy(this.gameObject);
                    return;
                }

                Debug.Log("invalid item in pick up");
            }
        }
    }
}