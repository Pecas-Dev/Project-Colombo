using ProjectColombo.LevelManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Tutorial
{
    public class TutorialDialogFunctions : MonoBehaviour
    {
        public GameObject chamberSlot;
        public List<GameObject> setActiveSlot;
        public List<GameObject> setInactiveSlot;
        public List<TutorialDummyBehavior> dummiesToAttack;
        public GameInputSystem.PlayerInputAction[] input;

        public void ActivateChamber()
        {
            chamberSlot.GetComponent<TileWorldChamber>().ActivateChamber();
        }

        public void DeactivateChamber()
        {
            chamberSlot.GetComponent<TileWorldChamber>().DeactivateChamber();
        }

        public void EndTutorial()
        {
            HealthManager hm = GameObject.Find("Player").GetComponent<HealthManager>();

            while (hm.currentHealth < hm.MaxHealth)
            {
                hm.Heal(hm.MaxHealth);
            }

            SceneManager.LoadScene(3);
        }

        public void SetActive()
        {
            foreach (var g in setActiveSlot)
            {
                g.SetActive(true);
            }
        }

        public void SetInactive()
        {
            foreach (var g in setInactiveSlot)
            {
                g.SetActive(false);
            }
        }

        public void DummiesAttack()
        {
            foreach (var e in dummiesToAttack)
            {
                e.SetAttacker();
            }
        }

        public void DummiesStopAttack()
        {
            foreach (var e in dummiesToAttack)
            {
                e.StopAttacker();
            }
        }

        public void EnableInput()
        {
            GameInputSO gameInput = GameManager.Instance.gameInput;
            gameInput.LockAllInputsViaTutorial();

            foreach (var i in input)
            {
                gameInput.UnlockInputViaTutorial(i);
            }
        }

        public void AddPotion()
        {
            GameManager.Instance.GetComponent<PlayerInventory>().numberOfPotions = 1;
        }

        public void AddGold()
        {
            var inventory = GameManager.Instance.GetComponent<PlayerInventory>();

            if (inventory.currencyAmount < 400)
            {
                inventory.currencyAmount = 400;
                CustomEvents.CoinsCollected(1);
            }
        }
    }
}
