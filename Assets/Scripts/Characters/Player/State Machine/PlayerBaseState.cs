using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine stateMachine;
        Matrix4x4 isometricMatrix;

        public PlayerBaseState(PlayerStateMachine playerStateMachine)
        {
            this.stateMachine = playerStateMachine;
            isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, stateMachine.Angle, 0));
        }
        protected Vector3 TransformDirectionToIsometric(Vector3 direction)
        {
            return isometricMatrix.MultiplyVector(direction).normalized;
        }

        protected void HandleStateSwitchFromInput()
        {
            //set attack states
            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.MajorAttack) && stateMachine.currentState != PlayerStateMachine.PlayerState.Attack)
            {

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;

                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.MinorAttack) && stateMachine.currentState != PlayerStateMachine.PlayerState.Attack)
            {

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;

                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }

            //set defense states
            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.Roll))
            {
                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToRoll)) return;

                stateMachine.SwitchState(new PlayerRollState(stateMachine));
                return;
            }

            if (stateMachine.gameInputSO.GetInputHeld(GameInputSystem.PlayerInputAction.Block) && stateMachine.currentState != PlayerStateMachine.PlayerState.Block)
            {
                stateMachine.SwitchState(new PlayerBlockState(stateMachine));
                return;
            }

            //set parry states
            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.MajorParry))
            {
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.MinorParry))
            {
                stateMachine.SwitchState(new PlayerParryState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }
        }
    }
}

