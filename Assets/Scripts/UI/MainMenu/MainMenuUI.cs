using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ProjectColombo.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public GameObject firstSelectedButton;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}