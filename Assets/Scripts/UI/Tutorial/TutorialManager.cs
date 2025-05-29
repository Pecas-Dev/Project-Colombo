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
                TutorialEvents.OnDummyHit -= AttackMinorEnemy;
                TutorialEvents.OnDummyHit += AttackMajorWithMajor;
                ShowDialog(dialogAfterAttackMinorEnemy);
                ResetPlayerState();
                counter = 0;
            }
        }

        private void AttackMajorWithMajor()
        {
            counter++;

            if (counter >= 2)
            {
                TutorialEvents.OnDummyHit -= AttackMajorWithMajor;
                TutorialEvents.OnDummyHit += AttackMajorWithMinor;
                ShowDialog(dialogAfterAttackMajorWithMajor);
                ResetPlayerState();
                counter = 0;
            }
        }

        private void AttackMajorWithMinor()
        {
            counter++;

            if (counter >= 4)
            {
                TutorialEvents.OnDummyHit -= AttackMajorWithMinor;
                CustomEvents.OnPlayerRoll += PlayerRolled;
                ShowDialog(dialogAfterAttackMajorWithMinor);
                ResetPlayerState();
                counter = 0;
            }
        }

        private void PlayerRolled()
        {
            counter++;

            if (counter >= 2)
            {
                CustomEvents.OnPlayerRoll -= PlayerRolled;
                CustomEvents.OnDamageBlocked += PlayerBlocked;
                ShowDialog(dialogAfterPlayerRoll);
                ResetPlayerState();
                counter = 0;
            }
        }

        private void PlayerBlocked(int arg1, GameGlobals.MusicScale arg2, HealthManager arg3)
        {
            CustomEvents.OnDamageBlocked -= PlayerBlocked;
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
            ShowDialog(dialogAfterPlayerBlocked);
            ResetPlayerState();
        }

        private void OnCoinsCollected(int obj)
        {
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
            ShowDialog(dialogAfterGoldPickUp);
        }

        private void OnCharmCollected(GameObject obj)
        {
            CustomEvents.OnCharmCollected -= OnCharmCollected;
            ShowDialog(dialogAfterPickUp);
        }

        private void OnPotionUsed()
        {
            CustomEvents.OnPotionUsed -= OnPotionUsed;
            potionText.SetActive(false);
            fightingChamber.GetComponent<TileWorldChamber>().DeactivateChamber();
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
                ShowDialog(dialogAfterParry);
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
            sm.myPlayerAnimator.PlayMovementAnimation();
            sm.RollInvincibleFrameStop();
            sm.InterruptAttack();
        }
    }
}