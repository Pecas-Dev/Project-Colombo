using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.DualShock;


public class IndicatorGlyphs : MonoBehaviour
{
    [Header("Glyph Assets")]
    [Tooltip("UI Image that displays the glyph")]
    [SerializeField] Image buttonImage;

    [SerializeField] Sprite playStationGlyph;

    [SerializeField] Sprite xboxGlyph;

    [SerializeField] Sprite keyboardGlyph;

    [Header("Dependencies")]
    [Tooltip("Any PlayerInput in the scene\n(WILL auto-search at runtime)")]
    [SerializeField] PlayerInput playerInput;

    void Awake()
    {
        if (playerInput == null)
        {
            playerInput = FindFirstObjectByType<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("PickupIndicatorGlyphs: No PlayerInput found – glyphs will not update.");
            return;
        }

        playerInput.onControlsChanged += OnControlsChanged;

        OnControlsChanged(playerInput);
    }

    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    void OnControlsChanged(PlayerInput playerInput)
    {
        if (playerInput.currentControlScheme == "KeyboardMouse")
        {
            SetGlyph(keyboardGlyph);
            return;
        }

        Gamepad pad = Gamepad.current;

        if (pad == null)
        {
            SetGlyph(keyboardGlyph);
            return;
        }

        if (pad is DualSenseGamepadHID || pad is DualShockGamepad)
        {
            SetGlyph(playStationGlyph);
            return;
        }

        if (pad is XInputController || pad is XInputControllerWindows)
        {
            SetGlyph(xboxGlyph);
            return;
        }

        SetGlyph(xboxGlyph);
    }

    void SetGlyph(Sprite glyph)
    {
        if (buttonImage == null) { return; }

        buttonImage.sprite = glyph;
        buttonImage.enabled = glyph != null;
    }
}
