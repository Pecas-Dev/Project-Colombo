using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects.Masks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace ProjectColombo.Objects
{
    public class PickUp : MonoBehaviour
    {
        public GameObject indicator;
        public bool active = false;
        ParticleSystem myParticles;
        BaseCharm myCharm;

        private void Start()
        {
            myParticles = GetComponent<ParticleSystem>();
            myCharm = GetComponentInChildren<BaseCharm>();
            indicator.SetActive(false);

            SetParticleColor();
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
                GameObject charm = myCharm.gameObject;
                GameManager.Instance.gameInput.ResetUseItemPressed();

                if (charm != null)
                {
                    CustomEvents.CharmCollected(charm);
                    Debug.Log("collected charm");
                    Destroy(this.gameObject);
                    return;
                }

                Debug.Log("invalid item in pick up");
            }
        }

        void SetParticleColor()
        {
            Color color;
            if (myCharm.charmRarity == RARITY.COMMON)
            {
                color = Color.cyan;
            }
            else if (myCharm.charmRarity == RARITY.RARE)
            {
                color = Color.yellow;
            }
            else
            {
                color = Color.magenta;
            }

            myParticles.Stop();
            var mainModule = myParticles.main;
            mainModule.startColor = color;
            myParticles.Play();
        }
    }
}