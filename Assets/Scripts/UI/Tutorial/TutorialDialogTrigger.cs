using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.UI;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialDialogTrigger : MonoBehaviour
    {
        public TutorialDialogSystem dialogSystem;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.gameInput.ResetMovementInput();
                other.GetComponent<PlayerStateMachine>().SwitchState(new PlayerMovementState(other.GetComponent<PlayerStateMachine>()));
                dialogSystem.EnableDialog();
            }
        }
    }
}