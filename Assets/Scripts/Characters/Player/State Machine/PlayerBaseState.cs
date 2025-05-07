using Unity.VisualScripting;
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

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;

                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.MinorAttackPressed && stateMachine.currentState != PlayerStateMachine.PlayerState.Attack)
            {

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;

                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }

            //set defense states
            if (stateMachine.gameInputSO.RollPressed)
            {

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToRoll)) return;

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
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.MinorParryPressed)
            {
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }
        }
    }
}

