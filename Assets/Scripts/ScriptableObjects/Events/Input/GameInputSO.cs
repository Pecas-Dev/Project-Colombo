using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Collections.AllocatorManager;



namespace ProjectColombo.GameInputSystem
{
    [Flags]
    public enum InputActionType
    {
        None = 0,
        Movement = 1 << 0,
        MajorAttack = 1 << 1,
        MinorAttack = 1 << 2,
        UseSpecialAbility = 1 << 3,
        Interact = 1 << 4,
        Roll = 1 << 5,
        Block = 1 << 6,
        MinorParry = 1 << 7,
        MajorParry = 1 << 8,
        Target = 1 << 9,
        TargetPoint = 1 << 10,
        UsePotion = 1 << 11,
        UseCharmAbility = 1 << 12,
        Pause = 1 << 13,
        All = ~0
    }



    [CreateAssetMenu(fileName = "GameInputSO", menuName = "Scriptable Objects/Input/GameInputSO")]
    public class GameInputSO : ScriptableObject
    {
        public Vector2 MovementInput { get; private set; } = Vector2.zero;
        public bool MajorAttackPressed { get; private set; } = false;
        public bool MinorAttackPressed { get; private set; } = false;
        public bool UseSpecialAbilityPressed { get; private set; } = false;
        public bool InteractPressed { get; private set; } = false;
        public bool RollPressed { get; private set; } = false;
        //public bool BlockPressed { get; private set; } = false;

        public bool MinorParryPressed { get; private set; } = false;
        public bool MajorParryPressed { get; private set; } = false;
        public bool TargetPressed { get; private set; } = false;
        public Vector2 TargetPointInput { get; private set; } = Vector2.zero;
        public bool UsePotionPressed { get; private set; } = false;
        public bool UseCharmAbilityPressed { get; private set; } = false;
        public bool PausePressed { get; private set; } = false;


        public bool CharmSwapPausePressed { get; private set; } = false;



        private Vector2 uiNavigateValue = Vector2.zero;
        public Vector2 UINavigateValue => uiNavigateValue;



        InputActionType allowedInputs = InputActionType.All;

        public InputSystem_Actions playerInputActions;

        public void Initialize()
        {
            if (playerInputActions != null)
            {
                return;
            }

            playerInputActions = new InputSystem_Actions();

            playerInputActions.Player.Enable();

            playerInputActions.Player.Movement.performed += OnMovePerformed;
            playerInputActions.Player.Movement.canceled += OnMoveCanceled;

            playerInputActions.Player.MajorAttack.performed += OnMajorAttackPerformed;
            playerInputActions.Player.MinorAttack.performed += OnMinorAttackPerformed;
            playerInputActions.Player.UseSpecialAbility.performed += OnUseSpecialAbilityPerformed;
            playerInputActions.Player.Interact.performed += OnInteractPerformed;

            playerInputActions.Player.Roll.performed += OnRollPerformed;
            playerInputActions.Player.MajorParry.performed += OnMajorParryPerformed;
            playerInputActions.Player.MinorParry.performed += OnMinorParryPerformed;

            playerInputActions.Player.Target.performed += OnTargetPerformed;
            playerInputActions.Player.TargetPoint.performed += OnTargetPointPerformed;
            playerInputActions.Player.TargetPoint.canceled += OnTargetPointCanceled;

            playerInputActions.Player.UsePotion.performed += OnUsePotionPerformed;
            playerInputActions.Player.UseCharmAbility.performed += OnUseCharmAbilityPerformed;

            playerInputActions.UI.Navigate.performed += OnNavigatePerformed;
            playerInputActions.UI.Navigate.canceled += OnNavigateCanceled;
        }

        public void Uninitialize()
        {
            if (playerInputActions == null) return;

            // Make sure to disable both action maps before uninitializing
            if (playerInputActions.UI.enabled)
            {
                playerInputActions.UI.Disable();
            }

            if (playerInputActions.Player.enabled)
            {
                playerInputActions.Player.Disable();
            }

            playerInputActions.Player.Movement.performed -= OnMovePerformed;
            playerInputActions.Player.Movement.canceled -= OnMoveCanceled;

            playerInputActions.Player.MajorAttack.performed -= OnMajorAttackPerformed;
            playerInputActions.Player.MinorAttack.performed -= OnMinorAttackPerformed;
            playerInputActions.Player.UseSpecialAbility.performed -= OnUseSpecialAbilityPerformed;
            playerInputActions.Player.Interact.performed -= OnInteractPerformed;

            playerInputActions.Player.Roll.performed -= OnRollPerformed;
            playerInputActions.Player.MajorParry.performed -= OnMajorParryPerformed;
            playerInputActions.Player.MinorParry.performed -= OnMinorParryPerformed;

            playerInputActions.Player.Target.performed -= OnTargetPerformed;
            playerInputActions.Player.TargetPoint.performed -= OnTargetPointPerformed;
            playerInputActions.Player.TargetPoint.canceled -= OnTargetPointCanceled;

            playerInputActions.Player.UsePotion.performed -= OnUsePotionPerformed;
            playerInputActions.Player.UseCharmAbility.performed -= OnUseCharmAbilityPerformed;



            playerInputActions.UI.Navigate.performed -= OnNavigatePerformed;
            playerInputActions.UI.Navigate.canceled -= OnNavigateCanceled;



            playerInputActions.Player.Disable();
            playerInputActions = null;
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
            ResetInput(inputAction);
        }

        public void EnableInput(InputActionType inputAction)
        {
            allowedInputs |= inputAction;
        }

        public void DisableAllInputsExcept(params InputActionType[] exceptions)
        {
            allowedInputs = InputActionType.None;

            // Build the new allowedInputs bitmask
            foreach (var input in exceptions)
            {
                allowedInputs |= input;
            }

            // Disable all inputs not in exceptions
            foreach (InputActionType input in Enum.GetValues(typeof(InputActionType)))
            {
                if (input == InputActionType.None)
                    continue;

                if (!exceptions.Contains(input))
                {
                    ResetInput(input); // You need to implement this
                }
            }
        }

        private void ResetInput(InputActionType input)
        {
            switch (input)
            {
            case InputActionType.Movement:
                    ResetMovementInput();
                        break;
            case InputActionType.MajorAttack:
                    ResetMajorAttackPressed();
                        break;
            case InputActionType.MinorAttack:
                    ResetMinorAttackPressed();
                        break;
            case InputActionType.UseSpecialAbility:
                    ResetUseSpecialAbilityPressed();
                        break;
            case InputActionType.Interact:
                    ResetUseItemPressed();
                        break;
            case InputActionType.Roll:
                    ResetRollPressed();
                        break;
            case InputActionType.MinorParry:
                    ResetMinorParryPressed();
                        break;
            case InputActionType.MajorParry:
                    ResetMajorParryPressed();
                        break;
            case InputActionType.Target:
                    ResetTargetPressed();
                        break;
            case InputActionType.UsePotion:
                    ResetUsePotion();
                        break;
            case InputActionType.UseCharmAbility:
                    ResetUseCharmAbility();
                        break;
            }
        }


        public bool IsInputEnabled(InputActionType inputAction)
        {
            return (allowedInputs & inputAction) != 0;
        }

        public void ResetMovementInput()
        {
            MovementInput = Vector2.zero;
        }

        public void ResetAllInputs()
        {
            //MovementInput               = Vector2.zero;
            MajorAttackPressed = false;
            MinorAttackPressed = false;
            UseSpecialAbilityPressed = false;
            InteractPressed = false;
            RollPressed = false;
            //BlockPressed                = false;
            MinorParryPressed = false;
            MajorParryPressed = false;
            TargetPressed = false;
            TargetPointInput = Vector2.zero;
            UsePotionPressed = false;
            UseCharmAbilityPressed = false;
            PausePressed = false;
        }

        public bool IsKeyboardInput()
        {
            return Keyboard.current != null && Keyboard.current.anyKey.isPressed;
        }

        public bool IsAnyInputActive()
        {
            return
                MovementInput.sqrMagnitude > 0.01f ||
                MajorAttackPressed ||
                MinorAttackPressed ||
                UseSpecialAbilityPressed ||
                InteractPressed ||
                RollPressed ||
                BlockPressed() ||
                MinorParryPressed ||
                MajorParryPressed ||
                TargetPressed ||
                TargetPointInput.sqrMagnitude > 0.01f ||
                UsePotionPressed ||
                UseCharmAbilityPressed ||
                PausePressed ||
                IsKeyboardInput();
        }



        // ################### MOVEMENT ##########################

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Movement)) return;

            Vector2 input = context.ReadValue<Vector2>();

            // Clamp stick noise
            if (input.magnitude < 0.1f)
            {
                MovementInput = Vector2.zero;
            }
            else
            {
                MovementInput = input.magnitude > 1f ? input.normalized : input;
            }
        }


        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Movement)) return;

            MovementInput = Vector2.zero;
        }

        // ########################################################


        //---------------------------------------------------------


        // ##################### Attacks ###########################
        void OnMajorAttackPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.MajorAttack)) return;

            MajorAttackPressed = true;
        }

        public void ResetMajorAttackPressed()
        {
            MajorAttackPressed = false;
        }

        void OnMinorAttackPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.MinorAttack)) return;

            MinorAttackPressed = true;
        }

        public void ResetMinorAttackPressed()
        {
            MinorAttackPressed = false;
        }
        // ########################################################


        //---------------------------------------------------------


        // ##################### Specials ###########################
        void OnUseSpecialAbilityPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.UseSpecialAbility)) return;

            UseSpecialAbilityPressed = true;
        }

        public void ResetUseSpecialAbilityPressed()
        {
            UseSpecialAbilityPressed = false;
        }

        void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Interact)) return;

            InteractPressed = true;
        }

        public void ResetUseItemPressed()
        {
            InteractPressed = false;
        }
        // ########################################################


        //---------------------------------------------------------


        // ###################### Defense ############################

        void OnRollPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.Roll)) return;

            RollPressed = true;
        }

        public void ResetRollPressed()
        {
            RollPressed = false;
        }

        public bool BlockPressed()
        {
            if (!IsInputEnabled(InputActionType.Block)) return false;

            return playerInputActions.Player.Block.IsPressed();
        }


        // ########################################################


        //---------------------------------------------------------

        // ###################### PARRY ############################

        void OnMinorParryPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.MinorParry)) return;

            MinorParryPressed = true;
        }

        public void ResetMinorParryPressed()
        {
            MinorParryPressed = false;
        }
        void OnMajorParryPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.MajorParry)) return;

            MajorParryPressed = true;
        }

        public void ResetMajorParryPressed()
        {
            MajorParryPressed = false;
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


        // ##################### Ability/ Item ###########################

        void OnUsePotionPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.UsePotion)) return;

            UsePotionPressed = true;
        }

        public void ResetUsePotion()
        {
            UsePotionPressed = false;
        }


        void OnUseCharmAbilityPerformed(InputAction.CallbackContext context)
        {
            if (!IsInputEnabled(InputActionType.UseCharmAbility)) return;

            UseCharmAbilityPressed = true;
        }

        public void ResetUseCharmAbility()
        {
            UseCharmAbilityPressed = false;
        }

        void OnPauseCharmSwapPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("CLICKEDDDDDDDDDDDDDD");
            CharmSwapPausePressed = true;
        }

        public void ResetCharmSwapPausePressed()
        {
            CharmSwapPausePressed = false;
        }



        // ########################################################


        //---------------------------------------------------------


        // ###################### UI ############################


        void OnNavigatePerformed(InputAction.CallbackContext context)
        {
            uiNavigateValue = context.ReadValue<Vector2>();

            if (uiNavigateValue.sqrMagnitude > 0.5f)
            {
            }
        }

        void OnNavigateCanceled(InputAction.CallbackContext context)
        {
            uiNavigateValue = Vector2.zero;
        }


        // ########################################################


        //---------------------------------------------------------

        public void EnableUIMode()
        {
            DisableInput(InputActionType.Movement);
            ResetAllInputs();

            if (playerInputActions != null)
            {
                if (playerInputActions.Player.enabled)
                {
                    playerInputActions.Player.Disable();
                }

                if (!playerInputActions.UI.enabled)
                {
                    playerInputActions.UI.Enable();
                }

                playerInputActions.UI.Navigate.Enable();
                playerInputActions.UI.Submit.Enable();
                playerInputActions.UI.MoveLeftShoulder.Enable();
                playerInputActions.UI.MoveRightShoulder.Enable();

                Debug.Log("<color=#00FFFF>[GameInputSO] UI mode enabled, Player disabled, UI Navigate enabled</color>");
            }
            else
            {
                Debug.LogWarning("Player input actions not initialized in EnableUIMode");
                Initialize();

                if (playerInputActions != null)
                {
                    if (playerInputActions.Player.enabled)
                    {
                        playerInputActions.Player.Disable();
                    }

                    if (!playerInputActions.UI.enabled)
                    {
                        playerInputActions.UI.Enable();
                    }

                    playerInputActions.UI.Navigate.Enable();
                    playerInputActions.UI.Submit.Enable();
                    playerInputActions.UI.MoveLeftShoulder.Enable();
                    playerInputActions.UI.MoveRightShoulder.Enable();
                }
            }
        }

        public void DisableUIMode()
        {
            if (playerInputActions != null)
            {
                if (playerInputActions.UI.enabled)
                {
                    playerInputActions.UI.Disable();
                }
                if (!playerInputActions.Player.enabled)
                {
                    playerInputActions.Player.Enable();
                }

                EnableInput(InputActionType.Movement);
            }
        }
    }
}