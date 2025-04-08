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

        [SerializeField] private GameInputSO gameInput;
        public GameObject pauseMenuUI;
        public GameObject firstSelectedButton;
        public bool gameIsPaused = false;

        private void Awake()
        {
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