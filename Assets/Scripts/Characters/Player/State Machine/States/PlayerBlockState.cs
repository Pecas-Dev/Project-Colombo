using ProjectColombo.GameInputSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


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
            Debug.Log("Player entered Blocking State");

            stateMachine.isBlocking = true;
            stateMachine.gameInputSO.DisableAllInputs();
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Movement);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Block);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Roll);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MajorAttack);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MinorAttack);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MajorParry);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.MinorParry);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.Pause);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.UseCharmAbility);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.UsePotion);
            stateMachine.gameInputSO.EnableInput(PlayerInputAction.UseSpecialAbility);

            stateMachine.StartCoroutine(StopMovement());
            stateMachine.myPlayerAnimator.TriggerBlock();
            stateMachine.myPlayerVFX.blockVFX.Play();
        }

        IEnumerator StopMovement()
        {
            yield return new WaitForFixedUpdate();

            //reset velocities
            stateMachine.myRigidbody.linearVelocity = Vector3.zero;
            stateMachine.myRigidbody.angularVelocity = Vector3.zero;
        }

        public override void Tick(float deltaTime)
        {
            Vector2 moveInput = stateMachine.gameInputSO.GetVector2Input(GameInputSystem.PlayerInputAction.Movement);

            if (moveInput.magnitude > 0.01f)
            {
                Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

                // Convert input to isometric space
                Vector3 isometricDirection = TransformDirectionToIsometric(moveDirection);

                if (isometricDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(isometricDirection);
                    stateMachine.myRigidbody.MoveRotation(Quaternion.Slerp(stateMachine.myRigidbody.rotation, targetRotation, Time.fixedDeltaTime * stateMachine.myEntityAttributes.rotationSpeedPlayer));
                }
            }


            HandleStateSwitchFromInput();

            if (/*!stateMachine.gameInputSO.GetInputHeld(PlayerInputAction.Block) || stateMachine.gameInputSO.GetInputReleased(PlayerInputAction.Block) ||*/ 
                (!IsSafeRawBlockInputHeld()))
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }
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

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.myPlayerAnimator.ResetBlock();
            stateMachine.isBlocking = false;

            stateMachine.myPlayerVFX.blockVFX.Stop();
        }
    }
}