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
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Dead);
            Debug.Log("Mommotti entered Death State");
        }

        public override void Tick(float deltaTime)
        {

        }

        public override void Exit()
        {
        }
    }
}