using ProjectColombo.GameInputSystem;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerParryState : PlayerBaseState
    {
        public PlayerParryState(PlayerStateMachine playerStateMachine, GameGlobals.MusicScale scale) : base(playerStateMachine)
        {
            stateMachine.myEntityAttributes.SetScale(scale);
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
        }

        public override void Exit()
        {
            stateMachine.myEntityAttributes.SetScale(GameGlobals.MusicScale.NONE);
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.ParryFrameStop();
        }
    }
}