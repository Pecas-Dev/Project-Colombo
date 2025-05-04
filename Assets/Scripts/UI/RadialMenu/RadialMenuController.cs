using UnityEngine;
using UnityEngine.InputSystem;

public class RadialMenuController : MonoBehaviour
{
    [Header("Radial Menu References")]
    public Canvas radialMenuCanvas;
    public RadialMenuShader radialMenuShader;

    [Header("Activation Settings")]
    public float holdTimeToActivate = 0.5f;


    InputSystem_Actions inputActions;


    bool isActivating = false;
    bool isRadialMenuActive = false;
    bool wasButtonPressed = false;

    float holdStartTime;


    void Start()
    {

        inputActions = new InputSystem_Actions();
        inputActions.UI.Enable();

        inputActions.UI.ActivateRadial.started += OnActivateRadialStarted;
        inputActions.UI.ActivateRadial.canceled += OnActivateRadialCanceled;

        if (radialMenuCanvas != null)
        {
            radialMenuCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("RadialMenuController: Radial menu canvas is not assigned!");
        }
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.UI.Disable();
            inputActions.Dispose();
        }
    }

    void Update()
    {
        if (isActivating && !isRadialMenuActive && wasButtonPressed)
        {
            float currentTime = Time.time;
            float timeHeld = currentTime - holdStartTime;

            if (timeHeld >= holdTimeToActivate)
            {
                ActivateRadialMenu();
            }

        }
    }

    void OnActivateRadialStarted(InputAction.CallbackContext context)
    {
        if (!isRadialMenuActive)
        {
            wasButtonPressed = true;
            isActivating = true;
            holdStartTime = Time.time;
        }
    }

    void OnActivateRadialCanceled(InputAction.CallbackContext context)
    {

        wasButtonPressed = false;
        isActivating = false;

        if (isRadialMenuActive)
        {
            DeactivateRadialMenu();
        }
    }

    void ActivateRadialMenu()
    {
        isRadialMenuActive = true;
        isActivating = false;

        if (radialMenuCanvas != null)
        {
            radialMenuCanvas.gameObject.SetActive(true);
        }
    }

    void DeactivateRadialMenu()
    {
        isRadialMenuActive = false;

        if (radialMenuCanvas != null)
        {
            radialMenuCanvas.gameObject.SetActive(false);
        }

        if (radialMenuShader != null)
        {
            int selectedSection = radialMenuShader.CurrentSelectedSection;

            if (selectedSection != -1)
            {
                Debug.Log($"Selected Section: {selectedSection}");
            }
        }
    }
}
