using UnityEngine;
using UnityEngine.SceneManagement;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerBlockState : PlayerBaseState
    {
        public PlayerBlockState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {

        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Block);
            stateMachine.myPlayerAnimator.TriggerBlock();
            Debug.Log("Player entered Blocking State");

            stateMachine.isBlocking = true;
            stateMachine.gameInputSO.DisableAllInputsExcept(GameInputSystem.InputActionType.Block, GameInputSystem.InputActionType.Roll);
        }

        public override void Tick(float deltaTime)
        {
            if (!stateMachine.gameInputSO.BlockPressed)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            if (stateMachine.gameInputSO.RollPressed)
            {
                stateMachine.SwitchState(new PlayerRollState(stateMachine));
                return;
            }
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.isBlocking = false;
        }
    }
}