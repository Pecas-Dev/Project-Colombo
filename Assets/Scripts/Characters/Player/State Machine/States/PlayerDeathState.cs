using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;

using UnityEngine;
using ProjectColombo.StateMachine.Mommotti;
using UnityEngine.SceneManagement;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerDeathState : PlayerBaseState
    {
        float timer = 0;
        public PlayerDeathState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {

        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Dead);
            stateMachine.myPlayerAnimator.TriggerDeath();
            Debug.Log("Player entered Death State");

            stateMachine.gameInputSO.DisableAllInputs();
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            if (timer > 2)
            {
                SceneManager.LoadScene(3);
            }
        }

        public override void Exit()
        {
        }
    }
}