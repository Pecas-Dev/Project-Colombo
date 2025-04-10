using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine stateMachine;

        public PlayerBaseState(PlayerStateMachine playerStateMachine)
        {
            this.stateMachine = playerStateMachine;
        }

        protected void HandleStateSwitchFromInput()
        {
            //set attack states
            if (stateMachine.gameInputSO.MajorAttackPressed && stateMachine.currentState != PlayerStateMachine.PlayerState.Attack)
            {
                stateMachine.gameInputSO.ResetMajorAttackPressed();
                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.MinorAttackPressed && stateMachine.currentState != PlayerStateMachine.PlayerState.Attack)
            {
                stateMachine.gameInputSO.ResetMinorAttackPressed();
                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }

            //set defense states
            if (stateMachine.gameInputSO.RollPressed)
            {
                stateMachine.gameInputSO.ResetRollPressed();
                stateMachine.SwitchState(new PlayerRollState(stateMachine));
                return;
            }

            if (stateMachine.gameInputSO.BlockPressed && stateMachine.currentState != PlayerStateMachine.PlayerState.Block)
            {
                stateMachine.SwitchState(new PlayerBlockState(stateMachine));
                return;
            }

            //set parry states
            if (stateMachine.gameInputSO.MajorParryPressed)
            {
                stateMachine.gameInputSO.ResetMajorParryPressed();
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.MinorParryPressed)
            {
                stateMachine.gameInputSO.ResetMinorParryPressed();
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }
        }
    }
}

