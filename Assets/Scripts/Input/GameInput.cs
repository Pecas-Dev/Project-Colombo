using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectColombo.Control
{
    public class GameInput : MonoBehaviour
    {
        public Vector2 MovementInput { get; private set; } = Vector2.zero;
        
        public bool AttackPressed { get; private set; } = false;
        public bool RollPressed { get; private set; } = false;


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
        }

        void OnDisable()
        {
            playerInputActions.Player.Move.performed -= OnMovePerformed;
            playerInputActions.Player.Move.canceled -= OnMoveCanceled;
            playerInputActions.Player.Attack.performed -= OnAttackPerformed;
            playerInputActions.Player.Roll.performed -= OnRollPerformed;
            playerInputActions.Player.Disable();
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();

            // Normalize input for consistent diagonal movement
            if (MovementInput.magnitude > 1f)
            {
                MovementInput = MovementInput.normalized;
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MovementInput = Vector2.zero;
        }

        void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPressed = true;
        }

        public void ResetAttackPressed()
        {
            AttackPressed = false;
        }

        void OnRollPerformed(InputAction.CallbackContext context)
        {
            RollPressed = true;
        }

        public void ResetRollPressed()
        {
            RollPressed = false;
        }
    }
}
