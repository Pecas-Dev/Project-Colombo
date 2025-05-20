using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
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
        public GameObject spawner;
        int currentDialog = -1;
        int dummiesHit = 0;

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
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (sameScale)
            {
                afterParry.gameObject.SetActive(true);
                afterParry.EnableDialog();
                spawner.SetActive(false);
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