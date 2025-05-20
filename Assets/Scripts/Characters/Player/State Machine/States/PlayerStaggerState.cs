using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;

using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerStaggerState : PlayerBaseState
    {
        float timer;


        public PlayerStaggerState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.currentComboString = "";
            stateMachine.myPlayerAnimator.TriggerStagger();
            stateMachine.gameInputSO.DisableAllInputsExcept(InputActionType.Pause);
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            if (timer >= stateMachine.myEntityAttributes.stunnedTime)
            {
                stateMachine.myPlayerAnimator.ResetStagger();
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            }

            if (!stateMachine.myPlayerAnimator.IsInStagger)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            }
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();

            if (stateMachine.gameInputSO.MovementInput.magnitude < 0.01f)
            {
                stateMachine.gameInputSO.ResetMovementInput();
            }
        }
    }
}