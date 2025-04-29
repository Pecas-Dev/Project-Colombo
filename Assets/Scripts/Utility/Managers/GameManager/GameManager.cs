using ProjectColombo.GameInputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.UI.Pausescreen;
using ProjectColombo.Inventory;
namespace ProjectColombo.GameManagement
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;


        public GameInputSO gameInput;
        public GameObject pauseMenuUI;
        public GameObject firstSelectedButton;
        public bool gameIsPaused = false;

        // SCENE NAMES

        static string MAIN_MENU = "00_MainMenu";

        //------------

        [Header("Transition")]
        [SerializeField] Animator transition;

        private void Awake()
        {
            //Cursor.visible = false;            // Hide the cursor
            //Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen (optional)

            gameInput.Initialize();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (gameInput.PausePressed && SceneManager.GetActiveScene().buildIndex != 0)
            {
                if (gameIsPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }

                gameInput.ResetPausePressed();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResumeGame();

            transition.Play("Open");

            // Reset input system properly on new scene load
            gameInput.Uninitialize();
            gameInput.Initialize();
            gameInput.EnableAllInputs();

            if (SceneManager.GetActiveScene().name == MAIN_MENU)
            {
                //reset stats to default when in main menu
                GetComponent<GlobalStats>().ResetStats();
                GetComponent<PlayerInventory>().Reset();
            }
        }



        public void PauseGame(bool showPause = true)
        {
            gameInput.EnableUIMode();
            Time.timeScale = 0;

            if (showPause)
            {
                pauseMenuUI.SetActive(true);
                pauseMenuUI.GetComponent<PauseMenuUI>().UpdateCharms();
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }

            gameIsPaused = true;
        }

        public void ResumeGame()
        {
            gameInput.DisableUIMode();
            gameInput.EnableAllInputs();

            if (SceneManager.GetActiveScene().name == MAIN_MENU)
            {
                gameInput.EnableUIMode();
            }

            Time.timeScale = 1;
            pauseMenuUI.SetActive(false);
            gameIsPaused = false;
        }
    }
}