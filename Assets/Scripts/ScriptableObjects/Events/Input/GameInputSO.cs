using UnityEngine;
using UnityEngine.InputSystem;



namespace ProjectColombo.GameInputSystem
{
    [CreateAssetMenu(fileName = "GameInputSO", menuName = "Scriptable Objects/Input/GameInputSO")]
    public class GameInputSO : ScriptableObject
    {
        public Vector2 MovementInput { get; private set; } = Vector2.zero;
        public Vector2 TargetPointInput { get; private set; } = Vector2.zero;

        public bool AttackPressed { get; private set; } = false;
        public bool RollPressed { get; private set; } = false;
        public bool TargetPressed { get; private set; } = false;
        public bool ParryPressed { get; private set; } = false;


        bool inputsDisabled = false;


        InputSystem_Actions playerInputActions;


        public void Initialize()
        {
            if (playerInputActions != null)
            {
                return;
            }

            playerInputActions = new InputSystem_Actions();

            playerInputActions.Player.Enable();

            playerInputActions.Player.Move.performed += OnMovePerformed;
            playerInputActions.Player.Move.canceled += OnMoveCanceled;

            playerInputActions.Player.Attack.performed += OnAttackPerformed;

            playerInputActions.Player.Roll.performed += OnRollPerformed;

            playerInputActions.Player.Parry.performed += OnParryPerformed;

            playerInputActions.Player.Target.performed += OnTargetPerformed;

            playerInputActions.Player.TargetPoint.performed += OnTargetPointPerformed;
            playerInputActions.Player.TargetPoint.canceled += OnTargetPointCanceled;
        }

        public void Uninitialize()
        {
            playerInputActions.Player.Move.performed -= OnMovePerformed;
            playerInputActions.Player.Move.canceled -= OnMoveCanceled;

            playerInputActions.Player.Attack.performed -= OnAttackPerformed;

            playerInputActions.Player.Roll.performed -= OnRollPerformed;

            playerInputActions.Player.Parry.performed += OnParryPerformed;

            playerInputActions.Player.Target.performed -= OnTargetPerformed;

            playerInputActions.Player.TargetPoint.performed -= OnTargetPointPerformed;
            playerInputActions.Player.TargetPoint.canceled -= OnTargetPointCanceled;

            playerInputActions.Player.Disable();
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
            return MovementInput.sqrMagnitude > 0.01f || AttackPressed || RollPressed || TargetPressed || ParryPressed || (Keyboard.current != null && Keyboard.current.anyKey.isPressed);
        }

        void ResetAllInputs()
        {
            MovementInput = Vector2.zero;
            TargetPointInput = Vector2.zero;
            AttackPressed = false;
            RollPressed = false;
            ParryPressed = false;
            //TargetPressed = false;
        }

        public void DisableAllInputsExceptRoll()
        {
            MovementInput = Vector2.zero;
            TargetPointInput = Vector2.zero;
            AttackPressed = false;
            ParryPressed = false;
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

        // ###################### PARRY ############################

        void OnParryPerformed(InputAction.CallbackContext context)
        {
            if (inputsDisabled) return;

            ParryPressed = true;
        }

        public void ResetParryPressed()
        {
            ParryPressed = false;
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
    }
}