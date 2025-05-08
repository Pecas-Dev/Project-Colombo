using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace ProjectColombo.UI
{
    public class UIInputSwitcher : MonoBehaviour
    {
        [SerializeField] GameObject firstSelectedButton;
        [SerializeField] float checkInterval = 0.1f;

        bool wasUsingMouse = false;
        bool isUsingController = false;

        PlayerInput playerInput;

        void Start()
        {
            playerInput = FindFirstObjectByType<PlayerInput>();

            if (firstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }

            StartCoroutine(MonitorInputDeviceChanges());
        }

        IEnumerator MonitorInputDeviceChanges()
        {
            while (true)
            {
                if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame || Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f))
                {
                    wasUsingMouse = true;
                    isUsingController = false;
                }

                if (Gamepad.current != null && (Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f || Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f))
                {
                    isUsingController = true;

                    if (wasUsingMouse && EventSystem.current.currentSelectedGameObject == null)
                    {
                        if (firstSelectedButton != null)
                        {
                            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                        }

                        wasUsingMouse = false;
                    }
                }

                yield return new WaitForSeconds(checkInterval);
            }
        }

        public void SetFirstSelectedButton(GameObject button)
        {
            firstSelectedButton = button;

            if (isUsingController && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }
        }
    }
}