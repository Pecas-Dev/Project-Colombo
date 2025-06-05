using ProjectColombo.GameManagement;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.Port;

namespace ProjectColombo.UI
{
    public class LooseScreen : MonoBehaviour
    {
        public TMP_Text text;
        public GameObject button;
        bool isActive = false;

        private void Start()
        {
            Color newColor = text.color;
            newColor.a = 0;
            text.color = newColor;
        }

        private void Update()
        {
            if (isActive)
            {
                if (GameManager.Instance.gameInput.GetInputPressed(GameInputSystem.PlayerInputAction.Interact))
                {
                    SceneManager.LoadScene(7);
                }
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void SetActive()
        {
            StartCoroutine(FadeIn());
        }

        public void Null()
        {

        }

        IEnumerator FadeIn()
        {
            float opacity = 0f;
            Color originalColor = text.color;

            while (opacity < 1f)
            {
                opacity += Time.deltaTime;
                opacity = Mathf.Clamp01(opacity);

                Color newColor = originalColor;
                newColor.a = opacity;
                text.color = newColor;

                yield return null;
            }

            isActive = true;
            button.SetActive(true);
            EventSystem.current.SetSelectedGameObject(button);
        }
    }
}