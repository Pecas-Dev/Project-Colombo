using ProjectColombo.GameManagement;
using UnityEngine;

namespace ProjectColombo.UI.Pausescreen
{
    public class PauseButtons : MonoBehaviour
    {
        public void MakeSelections()
        {
            GetComponentInParent<PauseMenuUI>().MakeSelection();
        }
    }
}