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
                dialogSystem.EnableDialog();
            }
        }
    }
}