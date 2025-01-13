using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ProjectColombo.Input
{
    public class GameInput : MonoBehaviour
    {
        public Vector2 MovementInput { get; private set; } = Vector2.zero;
        public Vector2 TargetPointInput { get; private set; } = Vector2.zero;

        public bool AttackPressed { get; private set; } = false;
        public bool RollPressed { get; private set; } = false;
        public bool TargetPressed { get; private set; } = false;


        //REMOVE THIS AND REFERENCES ASSOCIATED TO IT
        public bool CrouchPressed { get; private set; } = false;
        //-------------------------------------------------------------


        bool inputsDisabled = false;


        InputSystem_Actions playerInputActions;


        void Awake()
        {
            playerInputActions = new InputSystem_Actions();
        }

        void OnEnable()
        {
            playerInputActions.Player.Enable();

            playerInputActions.Player.Move.performed += OnMovePerformed;
            playerInputActions.Player.Move.canceled += OnMoveCanceled;

            playerInputActions.Player.Attack.performed += OnAttackPerformed;

            playerInputActions.Player.Roll.performed += OnRollPerformed;

            playerInputActions.Player.Target.performed += OnTargetPerformed;

            playerInputActions.Player.TargetPoint.performed += OnTargetPointPerformed;
            playerInputActions.Player.TargetPoint.canceled += OnTargetPointCanceled;

            //REMOVE THIS AND REFERENCES ASSOCIATED TO IT
            playerInputActions.Player.Crouch.performed += OnCrouchPerformed;
            playerInputActions.Player.Crouch.canceled += OnCrouchCanceled;
            //-------------------------------------------------------------
        }

        void OnDisable()
        {
            playerInputActions.Player.Move.performed -= OnMovePerformed;
            playerInputActions.Player.Move.canceled -= OnMoveCanceled;

            playerInputActions.Player.Attack.performed -= OnAttackPerformed;

            playerInputActions.Player.Roll.performed -= OnRollPerformed;

            playerInputActions.Player.Target.performed -= OnTargetPerformed;

            playerInputActions.Player.TargetPoint.performed -= OnTargetPointPerformed;
            playerInputActions.Player.TargetPoint.canceled -= OnTargetPointCanceled;

            playerInputActions.Player.Disable();

            //REMOVE THIS AND REFERENCES ASSOCIATED TO IT
            playerInputActions.Player.Crouch.performed -= OnCrouchPerformed;
            playerInputActions.Player.Crouch.canceled -= OnCrouchCanceled;
            //-------------------------------------------------------------
        }

        public void DisableInputs()
        {
            inputsDisabled = true;
            ResetAllInputs();
        }

        public void EnableInputs()
        {
            inputsDisabled = false;
        }

        public bool AreInputsDisabled()
        {
            return inputsDisabled;
        }

        public bool IsKeyboardInput()
        {
            return Keyboard.current != null && Keyboard.current.anyKey.isPressed;
        }

        public bool IsAnyInputActive()
        {
            return MovementInput.sqrMagnitude > 0.01f || AttackPressed || RollPressed || TargetPressed || (Keyboard.current != null && Keyboard.current.anyKey.isPressed);
        }

        void ResetAllInputs()
        {
            MovementInput = Vector2.zero;
            TargetPointInput = Vector2.zero;
            AttackPressed = false;
            RollPressed = false;
            //TargetPressed = false;
        }

        public void DisableAllInputsExceptRoll()
        {
            MovementInput = Vector2.zero;
            TargetPointInput = Vector2.zero;
            AttackPressed = false;
            //TargetPressed = false;
        }


        // ################### MOVEMENT ##########################

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            MovementInput = context.ReadValue<Vector2>();

            if (MovementInput.magnitude > 1f)
            {
                MovementInput = MovementInput.normalized;
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            MovementInput = Vector2.zero;
        }

        // ########################################################


        //---------------------------------------------------------


        // ##################### ATTACK ###########################
        void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            AttackPressed = true;
        }

        public void ResetAttackPressed()
        {
            AttackPressed = false;
        }

        // ########################################################


        //---------------------------------------------------------


        // ###################### ROLL ############################

        void OnRollPerformed(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            RollPressed = true;
        }

        public void ResetRollPressed()
        {
            RollPressed = false;
        }

        // ########################################################


        //---------------------------------------------------------


        // ##################### TARGET ###########################

        void OnTargetPerformed(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            TargetPressed = true;
        }

        public void ResetTargetPressed()
        {
            TargetPressed = false;
        }

        // ########################################################


        //---------------------------------------------------------


        // ################### TARGET-POINT #######################

        void OnTargetPointPerformed(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            TargetPointInput = context.ReadValue<Vector2>();
        }

        void OnTargetPointCanceled(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            TargetPointInput = Vector2.zero;
        }

        // ########################################################


        //---------------------------------------------------------


        // REMOVE THIS AND REFERENCES ASSOCIATED TO IT
        void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            CrouchPressed = true;
        }

        void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            CrouchPressed = false;
        }

        public void ResetCrouchPressed()
        {
            CrouchPressed = false;
        }
        //---------------------------------------------------------
    }
}
