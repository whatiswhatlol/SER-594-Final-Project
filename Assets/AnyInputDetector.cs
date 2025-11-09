using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AnyInputDetector : MonoBehaviour
{
    public UnityEvent OnAnyInput;

    void Update()
    {
        // --- Keyboard ---
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Trigger();
            return;
        }

        // --- Gamepad buttons ---
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.selectButton.wasPressedThisFrame ||
                Gamepad.current.leftShoulder.wasPressedThisFrame ||
                Gamepad.current.rightShoulder.wasPressedThisFrame ||
                Gamepad.current.leftTrigger.wasPressedThisFrame ||
                Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                Trigger();
                return;
            }

            // --- Gamepad stick or D-pad movement ---
            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f ||
                Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f ||
                Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.01f)
            {
                Trigger();
                return;
            }
        }

        // --- Mouse buttons or scroll (NOT movement) ---
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame ||
                Mouse.current.scroll.ReadValue().sqrMagnitude > 0.01f)
            {
                Trigger();
                return;
            }
        }

        // --- Touch ---
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Trigger();
            return;
        }
    }

    private void Trigger()
    {
        OnAnyInput?.Invoke();
        enabled = false;            
    }
}
