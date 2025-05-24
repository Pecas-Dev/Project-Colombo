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

            stateMachine.gameInputSO.DisableAllInputs();
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Pause);


            stateMachine.myPlayerAnimator.TriggerParry();
            stateMachine.myWeaponAttributes.PlayParryVFX();
        }

        public override void Tick(float deltaTime)
        {
            if (!stateMachine.myPlayerAnimator.IsInParry)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            }
            HandleStateSwitchFromInput();
        }

        public override void Exit()
        {
            stateMachine.myEntityAttributes.SetScale(GameGlobals.MusicScale.NONE);
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.ParryFrameStop();
            stateMachine.ParryPanaltyStop();
        }
    }
}