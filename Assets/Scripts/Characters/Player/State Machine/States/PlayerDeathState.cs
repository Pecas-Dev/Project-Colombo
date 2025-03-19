using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;

using UnityEngine;
using ProjectColombo.StateMachine.Mommotti;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerDeathState : PlayerBaseState
    {

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

        }

        public override void Exit()
        {
        }
    }
}