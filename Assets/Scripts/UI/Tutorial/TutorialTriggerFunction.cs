using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using UnityEngine;
using UnityEngine.Events;

public class TutorialTriggerFunction : MonoBehaviour
{
    public UnityEvent onTrigger;
    bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.gameInput.ResetMovementInput();
            other.GetComponent<PlayerStateMachine>().SwitchState(new PlayerMovementState(other.GetComponent<PlayerStateMachine>()));
            onTrigger?.Invoke();
            isTriggered = true;
            gameObject.SetActive(false);
        }
    }
}
