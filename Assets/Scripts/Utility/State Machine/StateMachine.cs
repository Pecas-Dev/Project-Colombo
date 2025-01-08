using UnityEngine;


namespace ProjectColombo.StateMachine
{
    public abstract class StateMachine : MonoBehaviour
    {
        State currentState;

        public void SwitchState(State newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        void Update()
        {
            currentState?.Tick(Time.deltaTime);
        }
    }
}
