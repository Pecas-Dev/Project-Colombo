using ProjectColombo.Combat;
using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.LevelManagement;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public List<GameObject> chambers;
        GameObject player;
        public TutorialSpawner startSpawner;
        int counter;

        public TutorialDialogSystem dialogAfterAttackMinorEnemy;
        public TutorialDialogSystem dialogAfterAttackMajorWithMajor;
        public TutorialDialogSystem dialogAfterAttackMajorWithMinor;
        public TutorialDialogSystem dialogAfterPlayerRoll;
        public TutorialDialogSystem dialogAfterPlayerBlocked;
        public TutorialDialogSystem dialogAfterParry;
        public TutorialDialogSystem dialogAfterPickUp;
        public TutorialDialogSystem dialogAfterGoldPickUp;

        public GameObject fightingChamber;
        public GameObject potionText;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");

            foreach (var chamber in chambers)
            {
                chamber.GetComponent<TileWorldChamber>().Initialize(new Vector2(0,0));
            }

            startSpawner.SpawnEnemy();

            GameInputSO gameInput = GameManager.Instance.gameInput;
            gameInput.LockAllInputsViaTutorial();
            gameInput.UnlockInputViaTutorial(PlayerInputAction.Movement);

            TutorialEvents.OnDummyHit += AttackMinorEnemy;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            CustomEvents.OnPotionUsed += OnPotionUsed;
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
            CustomEvents.OnCharmCollected += OnCharmCollected;
            CustomEvents.OnLevelChange += EndTutorial;
        }

        private void OnDestroy()
        {
            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
            CustomEvents.OnPotionUsed -= OnPotionUsed;
            CustomEvents.OnCharmCollected -= OnCharmCollected;
            CustomEvents.OnLevelChange -= EndTutorial;
            TutorialEvents.OnDummyHit -= AttackMinorEnemy;
            TutorialEvents.OnDummyHit -= AttackMajorWithMajor;
            TutorialEvents.OnDummyHit -= AttackMajorWithMinor;
            CustomEvents.OnPlayerRoll -= PlayerRolled;
            CustomEvents.OnDamageBlocked -= PlayerBlocked;
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
        }

        private void EndTutorial()
        {
            GameInputSO gameInput = GameManager.Instance.gameInput;
            gameInput.UnlockAllInputsViaTutorial();
            
            GameManager.Instance.GetComponent<PlayerInventory>().numberOfPotions = 1;
            player.GetComponent<HealthManager>().Heal(1000);
        }

        private void AttackMinorEnemy()
        {
            counter++;

            if (counter >= 4)
            {
                ShowDialog(dialogAfterAttackMinorEnemy);
                TutorialEvents.OnDummyHit -= AttackMinorEnemy;
                TutorialEvents.OnDummyHit += AttackMajorWithMajor;
                ResetPlayerState();
                counter = 0;
            }
        }

        private void AttackMajorWithMajor()
        {
            counter++;

            if (counter >= 4)
            {
                ShowDialog(dialogAfterAttackMajorWithMajor);
                TutorialEvents.OnDummyHit -= AttackMajorWithMajor;
                TutorialEvents.OnDummyHit += AttackMajorWithMinor;
                ResetPlayerState();
                counter = 0;
            }
        }

        private void AttackMajorWithMinor()
        {
            counter++;

            if (counter >= 4)
            {
                ShowDialog(dialogAfterAttackMajorWithMinor);
                TutorialEvents.OnDummyHit -= AttackMajorWithMinor;
                CustomEvents.OnPlayerRoll += PlayerRolled;
                ResetPlayerState();
                counter = 0;
            }
        }

        private void PlayerRolled()
        {
            counter++;

            if (counter >= 2)
            {
                ShowDialog(dialogAfterPlayerRoll);
                CustomEvents.OnPlayerRoll -= PlayerRolled;
                CustomEvents.OnDamageBlocked += PlayerBlocked;
                ResetPlayerState();
                counter = 0;
            }
        }

        private void PlayerBlocked(int arg1, GameGlobals.MusicScale arg2, HealthManager arg3)
        {
            ShowDialog(dialogAfterPlayerBlocked);
            CustomEvents.OnDamageBlocked -= PlayerBlocked;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            ResetPlayerState();
        }

        private void OnCoinsCollected(int obj)
        {
            ShowDialog(dialogAfterGoldPickUp);
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
        }

        private void OnCharmCollected(GameObject obj)
        {
            ShowDialog(dialogAfterPickUp);
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
                ShowDialog(dialogAfterParry);
                CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
            }
        }

        void ShowDialog(TutorialDialogSystem dialog)
        {
            dialog.gameObject.SetActive(true);
            dialog.EnableDialog();
        }


        void ResetPlayerState()
        {
            PlayerStateMachine sm = player.GetComponent<PlayerStateMachine>();
            sm.SwitchState(new PlayerMovementState(sm));
        }
    }
}