using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectColombo.GameInputSystem
{
    public enum PlayerInputAction
    {
        Movement,
        MajorAttack,
        MinorAttack,
        Roll,
        MajorParry,
        MinorParry,
        Block,
        UsePotion,
        UseCharmAbility,
        UseSpecialAbility,
        Interact,
        Target,
        TargetPoint,
        Pause,
        ActivateRadial
    }

    [CreateAssetMenu(fileName = "GameInputSO", menuName = "ScriptableObjects/GameInputSO")]
    public class GameInputSO : ScriptableObject
    {
        public InputSystem_Actions inputActions;
        private Dictionary<PlayerInputAction, InputAction> inputMap;

        private InputActionMap playerMap;
        private InputActionMap uiMap;

        private HashSet<PlayerInputAction> tutorialLockedInputs = new();

        public bool IsPlayerMapEnabled => playerMap.enabled;
        public bool IsUIMapEnabled => uiMap.enabled;

        public bool CharmSwapPausePressed { get; private set; }

        public void Initialize()
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();

            playerMap = inputActions.Player;
            uiMap = inputActions.UI;

            inputMap = new Dictionary<PlayerInputAction, InputAction>
            {
                { PlayerInputAction.Movement, inputActions.Player.Movement },
                { PlayerInputAction.MajorAttack, inputActions.Player.MajorAttack },
                { PlayerInputAction.MinorAttack, inputActions.Player.MinorAttack },
                { PlayerInputAction.Roll, inputActions.Player.Roll },
                { PlayerInputAction.MajorParry, inputActions.Player.MajorParry },
                { PlayerInputAction.MinorParry, inputActions.Player.MinorParry },
                { PlayerInputAction.Block, inputActions.Player.Block },
                { PlayerInputAction.UsePotion, inputActions.Player.UsePotion },
                { PlayerInputAction.UseCharmAbility, inputActions.Player.UseCharmAbility },
                { PlayerInputAction.UseSpecialAbility, inputActions.Player.UseSpecialAbility },
                { PlayerInputAction.Interact, inputActions.Player.Interact },
                { PlayerInputAction.Target, inputActions.Player.Target },
                { PlayerInputAction.TargetPoint, inputActions.Player.TargetPoint },
                { PlayerInputAction.ActivateRadial, inputActions.Player.ActivateRadial },
                { PlayerInputAction.Pause, inputActions.Player.Pause }
            };
        }

        public void Uninitialize()
        {
            inputActions?.Disable();
            inputMap?.Clear();
            tutorialLockedInputs.Clear();
        }

        public void EnableInput(PlayerInputAction action)
        {
            if (inputMap.TryGetValue(action, out var inputAction) && !tutorialLockedInputs.Contains(action))
                inputAction.Enable();
        }

        public void EnableAllInputs()
        {
            foreach (var kvp in inputMap)
            {
                if (!tutorialLockedInputs.Contains(kvp.Key))
                    kvp.Value.Enable();
            }
        }

        public void DisableInput(PlayerInputAction action)
        {
            if (inputMap.TryGetValue(action, out var inputAction))
                inputAction.Disable();
        }

        public void DisableAllInputs()
        {
            foreach (var inputAction in inputMap.Values)
            {
                inputAction.Disable();
            }
        }

        public bool GetInputPressed(PlayerInputAction action)
        {
            return inputMap.TryGetValue(action, out var inputAction)
                && inputAction.enabled
                && !tutorialLockedInputs.Contains(action)
                && inputAction.WasPressedThisFrame();
        }

        public bool GetInputHeld(PlayerInputAction action)
        {
            return inputMap.TryGetValue(action, out var inputAction)
                && inputAction.enabled
                && !tutorialLockedInputs.Contains(action)
                && inputAction.IsPressed();
        }

        public bool GetInputReleased(PlayerInputAction action)
        {
            return inputMap.TryGetValue(action, out var inputAction)
                && inputAction.enabled
                && !tutorialLockedInputs.Contains(action)
                && inputAction.WasReleasedThisFrame();
        }

        public Vector2 GetVector2Input(PlayerInputAction action)
        {
            return inputMap.TryGetValue(action, out var inputAction)
                && inputAction.enabled
                && !tutorialLockedInputs.Contains(action)
                ? inputAction.ReadValue<Vector2>()
                : Vector2.zero;
        }

        public void ResetAllInputs()
        {
            if (inputMap == null) return;

            foreach (var action in inputMap.Values)
            {
                action.Disable();
                action.Enable();
            }
        }

        public void SwitchToUI()
        {
            Debug.Log("SwitchToUI called!");

            playerMap.Disable();
            uiMap.Enable();

            Debug.Log($"playerMap.enabled: {playerMap.enabled}, uiMap.enabled: {uiMap.enabled}");
        }


        public void SwitchToGameplay()
        {
            Debug.Log("SwitchToGameplay called!");

            uiMap.Disable();
            playerMap.Enable();

            Debug.Log($"playerMap.enabled: {playerMap.enabled}, uiMap.enabled: {uiMap.enabled}");
        }

        public void LockAllInputsViaTutorial()
        {
            foreach (var action in inputMap.Keys)
            {
                tutorialLockedInputs.Add(action);
                DisableInput(action);
            }
        }

        public void LockInputViaTutorial(PlayerInputAction action)
        {
            tutorialLockedInputs.Add(action);
            DisableInput(action);
        }

        public void UnlockAllInputsViaTutorial()
        {
            foreach (var action in tutorialLockedInputs)
            {
                EnableInput(action);
            }
            tutorialLockedInputs.Clear();
        }


        public void UnlockInputViaTutorial(PlayerInputAction action)
        {
            tutorialLockedInputs.Remove(action);
            EnableInput(action);
        }

        public void UpdateInputs()
        {
            CharmSwapPausePressed = GetInputPressed(PlayerInputAction.ActivateRadial)
                                    || GetInputPressed(PlayerInputAction.Pause);
        }

        public void ResetCharmSwapPause()
        {
            CharmSwapPausePressed = false;
        }
    }
}
