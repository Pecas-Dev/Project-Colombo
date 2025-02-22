using System;
using UnityEngine;
using UnityEngine.InputSystem;



namespace ProjectColombo.GameInputSystem
{
    [Flags]
    public enum InputActionType
    {
        None = 0,
        Movement = 1 << 0,
        Attack = 1 << 1,
        Roll = 1 << 2,
        Parry = 1 << 3,
        Target = 1 << 4,
        TargetPoint = 1 << 5,
        Interact = 1 << 6,
        All = ~0
    }


    [CreateAssetMenu(fileName = "GameInputSO", menuName = "Scriptable Objects/Input/GameInputSO")]
    public class GameInputSO : ScriptableObject
    {
        public Vector2 MovementInput { get; private set; } = Vector2.zero;
        public Vector2 TargetPointInput { get; private set; } = Vector2.zero;

        public bool AttackPressed { get; private set; } = false;
        public bool RollPressed { get; private set; } = false;
        public bool TargetPressed { get; private set; } = false;
        public bool ParryPressed { get; private set; } = false;
        public bool InteractPressed { get; private set; } = false;



        InputActionType allowedInputs = InputActionType.All;


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

            playerInputActions.Player.Interact.performed += OnInteractPerformed;
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

        public void DisableAllInputs()
        {
            allowedInputs = InputActionType.None;
            ResetAllInputs();
        }

        public void EnableAllInputs()
        {
            allowedInputs = InputActionType.All;
        }

        public void DisableInput(InputActionType inputAction)
        {
            allowedInputs &= ~inputAction;
        }

        public void EnableInput(InputActionType inputAction)
        {
            allowedInputs |= inputAction;
        }

        public void DisableAllInputsExcept(params InputActionType[] exceptions)
        {
            allowedInputs = InputActionType.None;

            foreach (var input in exceptions)
            {
                allowedInputs |= input;
            }

            ResetAllInputs();
        }

        public bool IsInputEnabled(InputActionType inputAction)
        {
            return (allowedInputs & inputAction) != 0;
        }

        void ResetAllInputs()
        {
            MovementInput = Vector2.zero;
            TargetPointInput = Vector2.zero;
            AttackPressed = false;
            RollPressed = false;
            ParryPressed = false;
            TargetPressed = false;
        }

        public bool IsKeyboardInput()
        {
            return Keyboard.current != null && Keyboard.current.anyKey.isPressed;
        }

        public bool IsAnyInputActive()
        {
            return (MovementInput.sqrMagnitude > 0.01f || AttackPressed || RollPressed || TargetPressed || ParryPressed || IsKeyboardInput());
        }



        // ################### MOVEMENT ##########################

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Movement)) return;

            MovementInput = context.ReadValue<Vector2>();

            if (MovementInput.magnitude > 1f)
            {
                MovementInput = MovementInput.normalized;
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Movement)) return;

            MovementInput = Vector2.zero;
        }

        // ########################################################


        //---------------------------------------------------------


        // ##################### ATTACK ###########################
        void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Attack)) return;

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
            if (!IsInputEnabled(InputActionType.Roll)) return;

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
            if (!IsInputEnabled(InputActionType.Parry)) return;

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
            if (!IsInputEnabled(InputActionType.Target)) return;

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
            if (!IsInputEnabled(InputActionType.TargetPoint)) return;

            TargetPointInput = context.ReadValue<Vector2>();
        }

        void OnTargetPointCanceled(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.TargetPoint)) return;

            TargetPointInput = Vector2.zero;
        }

        // ########################################################


        //---------------------------------------------------------

        public void EnableUIMode()
        {
            DisableInput(InputActionType.Movement); // Disable player movement
            ResetAllInputs(); // Clear any active inputs
            playerInputActions.Player.Disable(); // Disable player controls
            playerInputActions.UI.Enable(); // Enable UI navigation
        }

        public void DisableUIMode()
        {
            playerInputActions.UI.Disable(); // Disable UI input
            playerInputActions.Player.Enable(); // Re-enable player controls
            EnableInput(InputActionType.Movement); // Allow movement again
        }

        void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Interact)) return;

            InteractPressed = true;
        }

        public void ResetInteractPressed()
        {
            InteractPressed = false;
        }

    }
}