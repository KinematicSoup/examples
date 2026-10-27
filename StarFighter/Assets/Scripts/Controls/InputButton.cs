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
    public enum MouseBinding { NONE, LEFT, RIGHT, MIDDLE, FORWARD, BACK }

    private static ButtonControl GetMouseButtonControl(Mouse mouse, MouseBinding button)
    {
        if (mouse == null)
        {
            return null;
        }

        switch (button)
        {
            case MouseBinding.LEFT:
                return mouse.leftButton;
            case MouseBinding.RIGHT:
                return mouse.rightButton;
            case MouseBinding.MIDDLE:
                return mouse.middleButton;
            case MouseBinding.FORWARD:
                return mouse.forwardButton;
            case MouseBinding.BACK:
                return mouse.backButton;
        }

        return null;
    }

    private static bool IsPressed(Key key)
    {
        if (key == Key.None)
        {
            return false;
        }

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[key] != null && keyboard[key].isPressed;
    }

    private static bool WasPressedThisFrame(Key key)
    {
        if (key == Key.None)
        {
            return false;
        }

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[key] != null && keyboard[key].wasPressedThisFrame;
    }

    private static bool WasReleasedThisFrame(Key key)
    {
        if (key == Key.None)
        {
            return false;
        }

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[key] != null && keyboard[key].wasReleasedThisFrame;
    }

    private static bool IsPressed(MouseBinding mouseButton)
    {
        ButtonControl button = GetMouseButtonControl(Mouse.current, mouseButton);
        return button != null && button.isPressed;
    }

    private static bool WasPressedThisFrame(MouseBinding mouseButton)
    {
        ButtonControl button = GetMouseButtonControl(Mouse.current, mouseButton);
        return button != null && button.wasPressedThisFrame;
    }

    private static bool WasReleasedThisFrame(MouseBinding mouseButton)
    {
        ButtonControl button = GetMouseButtonControl(Mouse.current, mouseButton);
        return button != null && button.wasReleasedThisFrame;
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
    public Key PcButton;
    public MouseBinding PcMouseButton;
    public Joystick JoystickButton;
    public string PcName
    {
        get
        {
            if (PcMouseButton != MouseBinding.NONE)
            {
                return PcMouseButton.ToString();
            }
            return PcButton.ToString();
        }
    }

    public InputButton(GameButton gameButton, string name, Key pcButton, MouseBinding pcMouseButton, GamePad.Button joystickButton)
    {
        GameButton = gameButton;
        Name = name;
        PcButton = pcButton;
        PcMouseButton = pcMouseButton;
        JoystickButton = new Joystick(joystickButton);
    }

    public InputButton(InputButton inputButton)
    {
        GameButton = inputButton.GameButton;
        Name = inputButton.Name;
        PcButton = inputButton.PcButton;
        PcMouseButton = inputButton.PcMouseButton;
        JoystickButton = new Joystick(inputButton.JoystickButton);
    }

    /*
     * returns true if any of the relevant keyboard or joystick keys are held down
     */
    public bool ButtonPressed()
    {
        if (IsPressed(PcButton) || IsPressed(PcMouseButton))
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
        if (WasPressedThisFrame(PcButton) || WasPressedThisFrame(PcMouseButton))
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
        if (WasReleasedThisFrame(PcButton) || WasReleasedThisFrame(PcMouseButton))
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
