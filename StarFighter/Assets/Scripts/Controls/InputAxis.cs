using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;

/*
 * stores all the mouse and joystick axis that are relevant to a specific in game command.
 * one mouse axis and one joystick axis are allowed
 */
public class InputAxis
{
    /*
     * stores an axis type input for a joystick.
     * only the x or y axis is read based on the AxisOrientation specified.
     */
    public class Joystick
    {
        public GamePad.Axis SelectedAxis;

        // returns the value of the relevant axis, and applies an exponent while preserving the +/-
        public float GetValue(float exponent)
        {
            float value = GamePad.GetAxis(SelectedAxis);
            return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), exponent);
        }

        // gets a display name for the axis
        public string Name
        {
            get
            {
                switch (SelectedAxis)
                {
                    case GamePad.Axis.L_STICK_X: return "L Stick X";
                    case GamePad.Axis.L_STICK_Y: return "L Stick Y";
                    case GamePad.Axis.R_STICK_X: return "R Stick X";
                    case GamePad.Axis.R_STICK_Y: return "R Stick Y";
                    case GamePad.Axis.DPAD_X:    return "Dpad X";
                    case GamePad.Axis.DPAD_Y:    return "Dpad Y";
                    case GamePad.Axis.TRIGGERS:  return "Triggers";
                }
                return "None";
            }
        }

        public Joystick(GamePad.Axis axis)
        {
            SelectedAxis = axis;
        }

        public Joystick(Joystick joystick)
        {
            SelectedAxis = joystick.SelectedAxis;
        }
    }


    /*
     * stores an axis type input for the mouse.
     */
    public class Mouse
    {
        public enum Axis { NONE, SCROLL_WHEEL, MOUSE_X, MOUSE_Y }
        public Axis SelectedAxis;
        private static Vector2 m_pos = Vector2.zero;

        // returns the value of the relevant axis
        public float Value
        {
            get
            {
                return GetAxis(SelectedAxis);
            }
        }

        // gets a display name for the axis
        public string Name
        {
            get
            {
                switch (SelectedAxis)
                {
                    case Axis.SCROLL_WHEEL: return "ScrollWheel";
                    case Axis.MOUSE_X:      return "Mouse X";
                    case Axis.MOUSE_Y:      return "Mouse Y";
                }
                return "None";
            }
        }

        public Mouse(Axis axis)
        {
            SelectedAxis = axis;
        }

        public Mouse(Mouse mouse)
        {
            SelectedAxis = mouse.SelectedAxis;
        }

        public static float GetAxis(Axis mouseAxis)
        {
            //float ret = 0.0f;
            float range = 2.0f * (float)Camera.main.pixelHeight;
            switch (mouseAxis)
            {
                case Axis.SCROLL_WHEEL: return Input.GetAxis("Mouse ScrollWheel") * 50;
                case Axis.MOUSE_X:
                    {
                        float input = Input.GetAxis("Mouse X");
                        float sign = input > 0 ? 1.0f : -1.0f;
                        input *= sign;
                        m_pos.x += sign * input * 30.0f;
                        m_pos.x = Mathf.Clamp(m_pos.x, -range, range);
                        return m_pos.x / range;

                    }
                case Axis.MOUSE_Y:
                    {
                        float input = Input.GetAxis("Mouse Y");
                        float sign = input > 0 ? 1.0f : -1.0f;
                        input *= sign;
                        m_pos.y += sign * input * 30.0f;
                        m_pos.y = Mathf.Clamp(m_pos.y, -range, range);
                        return m_pos.y / range;
                    }
            }
            return 0;
        }
    }


    public GameAxis GameAxis;
    public string Name;             // name as it appears in the controls binding
    public Mouse MouseAxis;
    public Joystick JoystickAxis;

    /*
     * returns the value of this axis as the mouse axis added to the joystick axis
     */
    public float GetValue(float exponent)
    {
        return MouseAxis.Value + JoystickAxis.GetValue(exponent);
    }

    public InputAxis(GameAxis gameAxis, string name, Mouse.Axis mouseAxis, GamePad.Axis joystickAxis)
    {
        GameAxis = gameAxis;
        Name = name;
        MouseAxis = new Mouse(mouseAxis);
        JoystickAxis = new Joystick(joystickAxis);
    }

    public InputAxis(InputAxis inputAxis)
    {
        GameAxis = inputAxis.GameAxis;
        Name = inputAxis.Name;
        MouseAxis = new Mouse(inputAxis.MouseAxis);
        JoystickAxis = new Joystick(inputAxis.JoystickAxis);
    }
}