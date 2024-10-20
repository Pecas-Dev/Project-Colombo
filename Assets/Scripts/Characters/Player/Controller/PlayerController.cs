using UnityEngine;
using ProjectColombo.Combat;
using ProjectColombo.Core;
using UnityEngine.InputSystem;

namespace ProjectColombo.Control
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float rotationSpeed = 720f; // Degrees per second

        Health health;
        Rigidbody playerRigidbody;


        InputSystem_Actions playerInputActions;

        Vector2 movementInput;
        Vector2 lookInput;

        void Awake()
        {
            health = GetComponent<Health>();
            playerRigidbody = GetComponent<Rigidbody>();
            playerInputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            playerInputActions.Player.Enable();
            playerInputActions.Player.Move.performed += OnMovePerformed;
            playerInputActions.Player.Move.canceled += OnMoveCanceled;
            playerInputActions.Player.Attack.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            playerInputActions.Player.Move.performed -= OnMovePerformed;
            playerInputActions.Player.Move.canceled -= OnMoveCanceled;
            playerInputActions.Player.Attack.performed -= OnAttackPerformed;
            playerInputActions.Player.Disable();
        }

        private void FixedUpdate()
        {
            if (health.GetIsDead()) return;

            Move();
            Rotate();
        }

        void Update()
        {
            UpdateAnimator();
        }

        private void Move()
        {
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y) * moveSpeed * Time.fixedDeltaTime;
            playerRigidbody.MovePosition(playerRigidbody.position + movement);
        }

        private void Rotate()
        {
            if (movementInput.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movementInput.x, 0, movementInput.y));
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                playerRigidbody.MoveRotation(newRotation);
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            movementInput = Vector2.zero;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            // Implement attack logic here
            GetComponent<Fight>().Attack();
        }

        void UpdateAnimator()
        {
            GetComponent<Animator>().SetFloat("speed", movementInput.magnitude * moveSpeed);
        }
    }
}
