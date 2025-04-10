using ProjectColombo.GameInputSystem;
using UnityEngine;


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
            stateMachine.gameInputSO.DisableAllInputsExcept(
                InputActionType.Block,
                InputActionType.Roll,
                InputActionType.MajorAttack,
                InputActionType.MinorAttack,
                InputActionType.MajorParry,
                InputActionType.MinorParry);

        }

        public override void Tick(float deltaTime)
        {
            HandleStateSwitchFromInput();

            if (!stateMachine.gameInputSO.BlockPressed)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
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