using ProjectColombo.GameInputSystem;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerParryState : PlayerBaseState
    {
        public PlayerParryState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Parry);

            stateMachine.gameInputSO.DisableAllInputsExcept(InputActionType.Roll);
            stateMachine.myPlayerAnimator.TriggerParry();
        }

        public override void Tick(float deltaTime)
        {
            if (!stateMachine.myPlayerAnimator.IsInParry)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            }

            //HandleAirPhysicsIfNeeded(deltaTime);
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.ParryFrameStop();
        }
    }
}