using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RadialMenuShader : MonoBehaviour
{
    [Header("Visual Settings")]
    public Image radialMenuImage;
    public Material radialMenuMaterial;

    [Header("Functionality")]
    public UnityEvent[] sectionEvents = new UnityEvent[3];

    [Header("Input Settings")]
    [SerializeField] float controllerDeadzone = 0.2f;
    [SerializeField] float mouseMoveThreshold = 5f;


    int currentSelectedSection = -1;


    RectTransform rectTransform;

    Canvas parentCanvas;
    Camera canvasCamera;

    InputSystem_Actions inputActions;

    Vector2 lastMousePosition;
    Vector2 lastGamepadInput;
    Vector2 currentInputPosition;

    bool isUsingGamepad = false;


    public int CurrentSelectedSection => currentSelectedSection;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            canvasCamera = parentCanvas.worldCamera;
        }
        else
        {
            canvasCamera = null;
        }

        if (radialMenuMaterial != null)
        {
            radialMenuImage.material = Instantiate(radialMenuMaterial);
        }

        inputActions = new InputSystem_Actions();
        inputActions.UI.Enable();

        inputActions.UI.NavigateRadial.performed += OnNavigateRadial;
        inputActions.UI.NavigateRadial.canceled += OnNavigateRadial;
        inputActions.UI.Click.performed += OnClick;

        lastMousePosition = Mouse.current.position.ReadValue();
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
        DetectActiveInput();

        if (isUsingGamepad)
        {
            HandleGamepadInput();
        }
        else
        {
            HandleMouseInput();
        }

        HandleRadialInput();
    }

    void DetectActiveInput()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        Vector2 mouseDelta = currentMousePosition - lastMousePosition;

        Vector2 currentGamepadInput = inputActions.UI.NavigateRadial.ReadValue<Vector2>();

        if (mouseDelta.magnitude > mouseMoveThreshold)
        {
            isUsingGamepad = false;
        }
        else if (currentGamepadInput.magnitude > controllerDeadzone)
        {
            isUsingGamepad = true;
        }

        lastMousePosition = currentMousePosition;
    }

    void HandleMouseInput()
    {
        currentInputPosition = Mouse.current.position.ReadValue();
    }

    void HandleGamepadInput()
    {
        Vector2 stickInput = inputActions.UI.NavigateRadial.ReadValue<Vector2>();

        if (stickInput.magnitude > controllerDeadzone)
        {
            Vector2 centerOfRadialMenu = GetRadialMenuCenterScreenSpace();

            float radius = rectTransform.rect.width / 2f;

            Vector2 scaledInput = stickInput * radius;

            currentInputPosition = centerOfRadialMenu + scaledInput;
        }
        else if (currentSelectedSection != -1)
        {
            Vector2 centerOfRadialMenu = GetRadialMenuCenterScreenSpace();

            float angle = (currentSelectedSection * 120f - 90f) * Mathf.Deg2Rad;
            float radius = rectTransform.rect.width / 2f * 0.7f;

            Vector2 positionForSection = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

            currentInputPosition = centerOfRadialMenu + positionForSection;
        }
    }

    void OnNavigateRadial(InputAction.CallbackContext context)
    {
    }

    Vector2 GetRadialMenuCenterScreenSpace()
    {
        Vector3 worldPosition = rectTransform.TransformPoint(rectTransform.rect.center);

        return RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPosition);
    }

    void HandleRadialInput()
    {
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, currentInputPosition, canvasCamera, out localPoint))
        {
            float distance = localPoint.magnitude;
            float radius = rectTransform.rect.width / 2f;

            if (distance > radius)
            {
                UpdateHighlight(-1);
                currentSelectedSection = -1;

                return;
            }

            float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

            if (angle < 0)
            {
                angle += 360;
            }

            int selectedSection = GetSectionFromAngle(angle);

            if (selectedSection != currentSelectedSection)
            {
                UpdateHighlight(selectedSection);
                currentSelectedSection = selectedSection;
            }
        }
        else if (isUsingGamepad && currentSelectedSection != -1)
        {
            UpdateHighlight(currentSelectedSection);
        }
        else
        {
            UpdateHighlight(-1);
            currentSelectedSection = -1;
        }
    }

    void OnClick(InputAction.CallbackContext context)
    {
        if (currentSelectedSection != -1)
        {
            SelectSection(currentSelectedSection);
        }
    }

    int GetSectionFromAngle(float angle)
    {
        float adjustedAngle = (angle + 90) % 360;

        if (adjustedAngle < 0)
        {
            adjustedAngle += 360;
        }

        int section = (int)(adjustedAngle / 120);

        return section;
    }

    void UpdateHighlight(int sectionIndex)
    {
        if (radialMenuImage != null && radialMenuImage.material != null)
        {
            radialMenuImage.material.SetInt("_HighlightedSection", sectionIndex);
        }
    }

    void SelectSection(int sectionIndex)
    {
        if (sectionIndex >= 0 && sectionIndex < sectionEvents.Length)
        {
            Debug.Log($"Selecting section: {sectionIndex}");
            sectionEvents[sectionIndex]?.Invoke();
        }
    }
}