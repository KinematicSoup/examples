using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;

/*
 * stores all the keyboard and joystick keys that are relevant to a specific in game command.
 */
public class InputButton
{
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
        if (Input.GetKey(PcButton))
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
        if (Input.GetKeyDown(PcButton))
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
        if (Input.GetKeyUp(PcButton))
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
