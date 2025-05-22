using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.UI
{
    public class DialogTrigger : MonoBehaviour
    {
        public DialogSystem dialogSystem;

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