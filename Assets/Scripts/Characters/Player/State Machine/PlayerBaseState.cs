using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


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
            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.MajorAttack) && stateMachine.currentStateEnum != PlayerStateMachine.PlayerState.Attack)
            {

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;

                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MAJOR));
                return;
            }

            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.MinorAttack) && stateMachine.currentStateEnum != PlayerStateMachine.PlayerState.Attack)
            {

                if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToAttack)) return;

                stateMachine.SwitchState(new PlayerAttackState(stateMachine, GameGlobals.MusicScale.MINOR));
                return;
            }

            //set defense states
            if (stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.Roll))
            {
                //if (!stateMachine.myStamina.HasEnoughStamina(stateMachine.myStamina.staminaToRoll)) return;
                if (!stateMachine.myStamina.TryConsumeStaminaWithFeedback(stateMachine.myStamina.staminaToRoll)) return;

                stateMachine.SwitchState(new PlayerRollState(stateMachine));
                return;
            }

            if (/*(stateMachine.gameInputSO.GetInputHeld(GameInputSystem.PlayerInputAction.Block) ||*/
                IsSafeRawBlockInputHeld() && stateMachine.currentStateEnum != PlayerStateMachine.PlayerState.Block)
            {
                stateMachine.SwitchState(new PlayerBlockState(stateMachine));
                return;
            }

            bool IsSafeRawBlockInputHeld()
            {
                if (!IsRawInputAllowed())
                {
                    return false;
                }

                return IsRawBlockInputHeld();
            }

            bool IsRawInputAllowed()
            {
                if (!stateMachine.gameInputSO.IsPlayerMapEnabled)
                {
                    return false;
                }

                bool blockActionEnabled = stateMachine.gameInputSO.GetInputHeld(GameInputSystem.PlayerInputAction.Block) ||stateMachine.gameInputSO.GetInputPressed(GameInputSystem.PlayerInputAction.Block) || stateMachine.gameInputSO.GetInputReleased(GameInputSystem.PlayerInputAction.Block);

                return true; 
            }

            bool IsRawBlockInputHeld()
            {
                bool gamepadL2Held = false;
                bool keyboardShiftHeld = false;

                if (Gamepad.current != null)
                {
                    gamepadL2Held = Gamepad.current.leftTrigger.isPressed;
                }

                if (Keyboard.current != null)
                {
                    keyboardShiftHeld = Keyboard.current.leftShiftKey.isPressed ||
                                       Keyboard.current.rightShiftKey.isPressed;
                }

                return gamepadL2Held || keyboardShiftHeld;
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

