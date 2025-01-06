using UnityEngine;
using UnityEngine.InputSystem;


namespace ProjectColombo.Control
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


        // ################### MOVEMENT ##########################

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();

            if (MovementInput.magnitude > 1f)
            {
                MovementInput = MovementInput.normalized;
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MovementInput = Vector2.zero;
        }

        // ########################################################


        //---------------------------------------------------------


        // ##################### ATTACK ###########################
        void OnAttackPerformed(InputAction.CallbackContext context)
        {
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
            TargetPointInput = context.ReadValue<Vector2>();
        }

        void OnTargetPointCanceled(InputAction.CallbackContext context)
        {
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
