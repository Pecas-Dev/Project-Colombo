using TMPro;
using UnityEngine;

namespace ProjectColombo.UI.Pausescreen
{
    public class PauseToggle : MonoBehaviour
    {
        public GameObject selection;
        public TMP_Text text;
        public bool selected = false;

        private void Start()
        {
            text.text = selection.name;
        }

        public void Toggle()
        {
            selected = !selected;
        }
    }
}