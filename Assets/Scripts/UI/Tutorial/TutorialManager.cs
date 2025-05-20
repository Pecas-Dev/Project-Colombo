using ProjectColombo.GameManagement;
using ProjectColombo.LevelManagement;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public List<GameObject> chambers;
        public TutorialSpawner startSpawner;

        [ReadOnlyInspector] public GameInputSystem.InputActionType[] allowedInputs;

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
        }

        private void Update()
        {
            GameManager.Instance.gameInput.DisableAllInputsExcept(allowedInputs);
        }
    }
}