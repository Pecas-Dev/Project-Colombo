using ProjectColombo.GameInputSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
namespace ProjectColombo.GameManagement
{
    public enum AllWeapons { SWORD };

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public AllWeapons playerWeapon;
        public List<GameObject> allWeapons;

        public GameInputSO gameInput;
        public GameObject pauseMenuUI;
        public GameObject firstSelectedButton;
        public bool gameIsPaused = false;

        // SCENE NAMES

        static string MAIN_MENU = "2_MainMenu";
        static string MAIN_MENU_2 = "00_MainMenu";

        //------------

        private void Awake()
        {
            //Cursor.visible = false;            // Hide the cursor
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen (optional)
            
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

            if(SceneManager.GetActiveScene().name == MAIN_MENU || SceneManager.GetActiveScene().name == MAIN_MENU_2)
            {
                gameInput.EnableUIMode();
            }

            if (gameInput.playerInputActions.Player.enabled == false)
            {
                Debug.Log("Player is dead now, come back later");
            }

            if (gameInput.playerInputActions.UI.enabled == true)
            {
                Debug.Log("It's UI Time! (:");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResumeGame();

            // Reset input system properly on new scene load
            gameInput.Uninitialize();
            gameInput.Initialize();
            gameInput.EnableAllInputs();
        }

        public GameObject GetMyWeapon()
        {
            switch (playerWeapon)
            {
            case AllWeapons.SWORD:
                return allWeapons[0];
            }

            return null;
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
            pauseMenuUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            gameIsPaused = true;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            pauseMenuUI.SetActive(false);
            gameIsPaused = false;
        }
    }
}