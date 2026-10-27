using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/*
 * stores all the keyboard and joystick keys that are relevant to a specific in game command.
 */
public class InputButton
{
    private static bool TryGetMouseControl(KeyCode keyCode, out ButtonControl button)
    {
        button = null;
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        switch (keyCode)
        {
            case KeyCode.Mouse0: button = mouse.leftButton; return true;
            case KeyCode.Mouse1: button = mouse.rightButton; return true;
            case KeyCode.Mouse2: button = mouse.middleButton; return true;
            case KeyCode.Mouse3: button = mouse.forwardButton; return true;
            case KeyCode.Mouse4: button = mouse.backButton; return true;
        }

        return false;
    }
    private static bool TryConvertKeyCodeToKey(KeyCode keyCode, out Key key)
    {
        switch (keyCode)
        {
            case KeyCode.None: key = Key.None; return false;
            case KeyCode.Return: key = Key.Enter; return true;
            case KeyCode.KeypadEnter: key = Key.NumpadEnter; return true;
            case KeyCode.LeftShift: key = Key.LeftShift; return true;
            case KeyCode.RightShift: key = Key.RightShift; return true;
            case KeyCode.LeftControl: key = Key.LeftCtrl; return true;
            case KeyCode.RightControl: key = Key.RightCtrl; return true;
            case KeyCode.LeftAlt: key = Key.LeftAlt; return true;
            case KeyCode.RightAlt: key = Key.RightAlt; return true;
            case KeyCode.LeftCommand: key = Key.LeftMeta; return true;
            case KeyCode.RightCommand: key = Key.RightMeta; return true;
            case KeyCode.Alpha0: key = Key.Digit0; return true;
            case KeyCode.Alpha1: key = Key.Digit1; return true;
            case KeyCode.Alpha2: key = Key.Digit2; return true;
            case KeyCode.Alpha3: key = Key.Digit3; return true;
            case KeyCode.Alpha4: key = Key.Digit4; return true;
            case KeyCode.Alpha5: key = Key.Digit5; return true;
            case KeyCode.Alpha6: key = Key.Digit6; return true;
            case KeyCode.Alpha7: key = Key.Digit7; return true;
            case KeyCode.Alpha8: key = Key.Digit8; return true;
            case KeyCode.Alpha9: key = Key.Digit9; return true;
            case KeyCode.Keypad0: key = Key.Numpad0; return true;
            case KeyCode.Keypad1: key = Key.Numpad1; return true;
            case KeyCode.Keypad2: key = Key.Numpad2; return true;
            case KeyCode.Keypad3: key = Key.Numpad3; return true;
            case KeyCode.Keypad4: key = Key.Numpad4; return true;
            case KeyCode.Keypad5: key = Key.Numpad5; return true;
            case KeyCode.Keypad6: key = Key.Numpad6; return true;
            case KeyCode.Keypad7: key = Key.Numpad7; return true;
            case KeyCode.Keypad8: key = Key.Numpad8; return true;
            case KeyCode.Keypad9: key = Key.Numpad9; return true;
            case KeyCode.CapsLock: key = Key.CapsLock; return true;
            case KeyCode.Numlock: key = Key.NumLock; return true;
            case KeyCode.Print: key = Key.PrintScreen; return true;
            case KeyCode.ScrollLock: key = Key.ScrollLock; return true;
            case KeyCode.Menu: key = Key.ContextMenu; return true;
        }

        return System.Enum.TryParse(keyCode.ToString(), true, out key);
    }

    private static bool TryGetKeyboardControl(KeyCode keyCode, out KeyControl keyControl)
    {
        keyControl = null;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        Key key;
        if (!TryConvertKeyCodeToKey(keyCode, out key))
        {
            return false;
        }

        keyControl = keyboard[key];
        return keyControl != null;
    }

    private static bool IsPressed(KeyCode keyCode)
    {
        ButtonControl mouseControl;
        if (TryGetMouseControl(keyCode, out mouseControl))
        {
            return mouseControl.isPressed;
        }

        KeyControl keyControl;
        if (TryGetKeyboardControl(keyCode, out keyControl))
        {
            return keyControl.isPressed;
        }

        return false;
    }
    private static bool WasPressedThisFrame(KeyCode keyCode)
    {
        ButtonControl mouseControl;
        if (TryGetMouseControl(keyCode, out mouseControl))
        {
            return mouseControl.wasPressedThisFrame;
        }

        KeyControl keyControl;
        if (TryGetKeyboardControl(keyCode, out keyControl))
        {
            return keyControl.wasPressedThisFrame;
        }

        return false;
    }

    private static bool WasReleasedThisFrame(KeyCode keyCode)
    {
        ButtonControl mouseControl;
        if (TryGetMouseControl(keyCode, out mouseControl))
        {
            return mouseControl.wasReleasedThisFrame;
        }

        KeyControl keyControl;
        if (TryGetKeyboardControl(keyCode, out keyControl))
        {
            return keyControl.wasReleasedThisFrame;
        }

        return false;
    }

    /*
     * stores a button type input for a joystick.
     * a button, trigger, or axis is only relevant if it matches the type specified.
     */
    public class Joystick
    {
        public GamePad.Button Button;

        private bool m_pressedLastFrame = false;
        public bool PressedLastFrame
        {
            get { return m_pressedLastFrame; }
        }

        public Joystick(GamePad.Button button)
        {
            Button = button;
        }

        public Joystick(Joystick joystick)
        {
            Button = joystick.Button;
        }

        // returns true if the button is down
        public bool ButtonPressed()
        {
            return GamePad.GetButton(Button);
        }
        
        // returns true when the button is first down
        public bool ButtonDown()
        {
            return !m_pressedLastFrame && ButtonPressed();
        }
        
        // returns true when the button is lifted
        public bool ButtonUp()
        {
            return m_pressedLastFrame && !ButtonPressed();
        }

        // makes the button record if it was pressed last frame. Must run at the end of every frame
        public void Update()
        {
            m_pressedLastFrame = ButtonPressed();
        }

        // gets a display name for the axis
        public string Name
        {
            get
            {
                switch (Button)
                {
                    case GamePad.Button.A:           return "A";
                    case GamePad.Button.B:           return "B";
                    case GamePad.Button.X:           return "X";
                    case GamePad.Button.Y:           return "Y";
                    case GamePad.Button.R_SHOULDER:  return "R Bumper";
                    case GamePad.Button.L_SHOULDER:  return "L Bumper";
                    case GamePad.Button.BACK:        return "Back";
                    case GamePad.Button.START:       return "Start";
                    case GamePad.Button.L_STICK:     return "L Stick";
                    case GamePad.Button.R_STICK:     return "R Stick";

                    case GamePad.Button.L_TRIGGER:   return "L Trigger";
                    case GamePad.Button.R_TRIGGER:   return "R Trigger";
                    case GamePad.Button.DPAD_UP:     return "Dpad Up";
                    case GamePad.Button.DPAD_DOWN:   return "Dpad Down";
                    case GamePad.Button.DPAD_LEFT:   return "Dpad Left";
                    case GamePad.Button.DPAD_RIGHT:  return "Dpad Right";
                    case GamePad.Button.L_STICK_UP:    return "L Stick Up";
                    case GamePad.Button.L_STICK_DOWN:  return "L Stick Down";
                    case GamePad.Button.L_STICK_LEFT:  return "L Stick Left";
                    case GamePad.Button.L_STICK_RIGHT: return "L Stick Right";
                    case GamePad.Button.R_STICK_UP:    return "R Stick Up";
                    case GamePad.Button.R_STICK_DOWN:  return "R Stick Down";
                    case GamePad.Button.R_STICK_LEFT:  return "R Stick Left";
                    case GamePad.Button.R_STICK_RIGHT: return "R Stick Right";
                }
                return "None";
            }
        }
    }


    public GameButton GameButton;
    public string Name;             // name as it appears in the controls binding Ex) Primary Fire
    public KeyCode PcButton;
    public Joystick JoystickButton;

    public InputButton(GameButton gameButton, string name, KeyCode pcButton, GamePad.Button joystickButton)
    {
        GameButton = gameButton;
        Name = name;
        PcButton = pcButton;
        JoystickButton = new Joystick(joystickButton);
    }

    public InputButton(InputButton inputButton)
    {
        GameButton = inputButton.GameButton;
        Name = inputButton.Name;
        PcButton = inputButton.PcButton;
        JoystickButton = new Joystick(inputButton.JoystickButton);
    }

    /*
     * returns true if any of the relevant keyboard or joystick keys are held down
     */
    public bool ButtonPressed()
    {
        if (IsPressed(PcButton))
        {
            return true;
        }
        if (JoystickButton.ButtonPressed())
        {
            return true;
        }
        return false;
    }

    /*
     * returns true on the first frame a relevant keyboard or joystick key is pressed
     */
    public bool ButtonDown()
    {
        if (WasPressedThisFrame(PcButton))
        {
            return true;
        }
        if (JoystickButton.ButtonDown())
        {
            return true;
        }
        return false;
    }

    /*
     * returns true when any relevant keyboard or joystick key is released
     */
    public bool ButtonUp()
    {
        if (WasReleasedThisFrame(PcButton))
        {
            return true;
        }
        if (JoystickButton.ButtonUp())
        {
            return true;
        }
        return false;
    }

    /*
     * run at the end of every frame to make the ButtonUp and ButtonDown funtions work as intended
     */
    public void Update()
    {
        JoystickButton.Update();
    }
}
