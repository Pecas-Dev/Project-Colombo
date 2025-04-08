using UnityEngine;
using UnityEngine.SceneManagement;


namespace ProjectColombo.StateMachine.GameManage
{
    public class GameManagerStateMachine : StateMachine
    {
        public enum GameStateType
        {
            MainMenu,
            Cinematics,
            Gameplay,
            Pause,
            GameOver
        }

        [Header("Persistent Manager")]
        static bool isInitialized = false;

        [field: SerializeField, ReadOnlyInspector] public GameStateType CurrentGameState { get; private set; }

        void Awake()
        {
            if (!isInitialized)
            {
                DontDestroyOnLoad(gameObject);
                isInitialized = true;

                SwitchState(new GameStateMainMenu(this));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetGameState(GameStateType newState)
        {
            CurrentGameState = newState;
            Debug.Log("Switched to Game State: " + newState);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
