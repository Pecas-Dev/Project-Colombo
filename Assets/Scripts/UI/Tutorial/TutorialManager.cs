using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.LevelManagement;
using ProjectColombo.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public List<GameObject> chambers;
        public TutorialSpawner startSpawner;

        [ReadOnlyInspector] public GameInputSystem.InputActionType[] allowedInputs;
        public List<TutorialDialogSystem> dialogsInOrder;
        public TutorialDialogSystem afterParry;
        public TutorialDialogSystem afterPickUp;
        int currentDialog = -1;
        int dummiesHit = 0;
        public GameObject fightingChamber;
        public GameObject potionText;

        private void Start()
        {
            foreach (var chamber in chambers)
            {
                chamber.GetComponent<TileWorldChamber>().Initialize(new Vector2(0,0));
            }

            startSpawner.SpawnEnemy();

            allowedInputs = new GameInputSystem.InputActionType[]
            {
                GameInputSystem.InputActionType.Movement,
                GameInputSystem.InputActionType.Roll
            };

            TutorialEvents.OnDummyHit += OnDummyHit;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            CustomEvents.OnPotionUsed += OnPotionUsed;
            CustomEvents.OnCharmCollected += OnCharmCollected;
            CustomEvents.OnLevelChange += EndTutorial;
        }

        private void EndTutorial()
        {
            GameManager.Instance.GetComponent<PlayerInventory>().numberOfPotions = 1;
            GameObject.Find("Player").GetComponent<HealthManager>().Heal(1000);
            TutorialEvents.OnDummyHit -= OnDummyHit;
            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
            CustomEvents.OnPotionUsed -= OnPotionUsed;
            CustomEvents.OnCharmCollected -= OnCharmCollected;
            CustomEvents.OnLevelChange -= EndTutorial;
        }

        private void OnCharmCollected(GameObject obj)
        {
            afterPickUp.gameObject.SetActive(true);
            afterPickUp.EnableDialog();
            CustomEvents.OnCharmCollected -= OnCharmCollected;
        }

        private void OnPotionUsed()
        {
            potionText.SetActive(false);
            fightingChamber.GetComponent<TileWorldChamber>().DeactivateChamber();
            CustomEvents.OnPotionUsed -= OnPotionUsed;
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                afterParry.gameObject.SetActive(true);
                afterParry.EnableDialog();
                CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
            }
        }

        private void OnDummyHit()
        {
            dummiesHit++;

            if (dummiesHit >= 3)
            {
                dummiesHit = 0;
                currentDialog++;
                NextDialog();
            }
        }

        void NextDialog()
        {
            dialogsInOrder[currentDialog].gameObject.SetActive(true);
            dialogsInOrder[currentDialog].EnableDialog();

            if (currentDialog == dialogsInOrder.Count - 1)
            {
                TutorialEvents.OnDummyHit -= OnDummyHit;
            }
        }

        private void Update()
        {
            GameManager.Instance.gameInput.DisableAllInputsExcept(allowedInputs);
        }
    }
}