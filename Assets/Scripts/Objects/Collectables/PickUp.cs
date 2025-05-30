using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Objects.Charms;
using TMPro;
using UnityEngine;

namespace ProjectColombo.Objects
{
    public class PickUp : MonoBehaviour
    {
        public GameObject indicator;
        public bool active = false;
        public TMP_Text pickUpText;
        [HideInInspector] public GameObject myCharm;

        private void Start()
        {
            indicator.SetActive(false);
        }

        public void SetCharm(GameObject charm)
        {
            myCharm = charm;

            string charmName = myCharm.GetComponent<BaseCharm>().charmName;
            pickUpText.text =  charmName;//"to pick up " +
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
            if (active && GameManager.Instance.gameInput.GetInputPressed(GameInputSystem.PlayerInputAction.Interact))
            {
                GameObject charm = GetComponentInChildren<BaseCharm>().gameObject;
                CustomEvents.CharmCollected(charm);
                gameObject.SetActive(false);
                //Destroy(this.gameObject);
                return;
            }
        }
    }
}