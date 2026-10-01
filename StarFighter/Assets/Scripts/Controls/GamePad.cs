using UnityEngine;
using System.Collections;

/*
 * manages input from a game controller
 */
namespace GamepadInput
{
    public static class GamePad
    {
        public enum Button { NONE, A, B, Y, X, R_SHOULDER, L_SHOULDER, R_STICK, L_STICK, BACK, START, L_TRIGGER, R_TRIGGER, DPAD_UP, DPAD_DOWN, DPAD_LEFT, DPAD_RIGHT, L_STICK_UP, L_STICK_DOWN, L_STICK_LEFT, L_STICK_RIGHT, R_STICK_UP, R_STICK_DOWN, R_STICK_LEFT, R_STICK_RIGHT }
        public enum Axis { NONE, L_STICK_X, L_STICK_Y, R_STICK_X, R_STICK_Y, DPAD_X, DPAD_Y, TRIGGERS }

        public static bool GetButton(Button button)
        {
            switch (button)
            {
                case Button.A:          return Input.GetKey(KeyCode.JoystickButton0);
                case Button.B:          return Input.GetKey(KeyCode.JoystickButton1);
                case Button.X:          return Input.GetKey(KeyCode.JoystickButton2);
                case Button.Y:          return Input.GetKey(KeyCode.JoystickButton3);
                case Button.R_SHOULDER: return Input.GetKey(KeyCode.JoystickButton5);
                case Button.L_SHOULDER: return Input.GetKey(KeyCode.JoystickButton4);
                case Button.BACK:       return Input.GetKey(KeyCode.JoystickButton6);
                case Button.START:      return Input.GetKey(KeyCode.JoystickButton7);
                case Button.L_STICK:    return Input.GetKey(KeyCode.JoystickButton8);
                case Button.R_STICK:    return Input.GetKey(KeyCode.JoystickButton9);

                case Button.L_TRIGGER:   return Input.GetAxis("TriggersL") > 0.3f;
                case Button.R_TRIGGER:   return Input.GetAxis("TriggersR") > 0.3f;
                case Button.DPAD_UP:     return GamePad.GetAxis(Axis.DPAD_Y) > 0.5f;
                case Button.DPAD_DOWN:   return GamePad.GetAxis(Axis.DPAD_Y) < -0.5f;
                case Button.DPAD_LEFT:   return GamePad.GetAxis(Axis.DPAD_X) < -0.5f;
                case Button.DPAD_RIGHT:  return GamePad.GetAxis(Axis.DPAD_X) > 0.5f;
                case Button.L_STICK_UP:    return GamePad.GetAxis(Axis.L_STICK_Y) < -0.5f;
                case Button.L_STICK_DOWN:  return GamePad.GetAxis(Axis.L_STICK_Y) > 0.5f;
                case Button.L_STICK_LEFT:  return GamePad.GetAxis(Axis.L_STICK_X) < -0.5f;
                case Button.L_STICK_RIGHT: return GamePad.GetAxis(Axis.L_STICK_X) > 0.5f;
                case Button.R_STICK_UP:    return GamePad.GetAxis(Axis.R_STICK_Y) < -0.5f;
                case Button.R_STICK_DOWN:  return GamePad.GetAxis(Axis.R_STICK_Y) > 0.5f;
                case Button.R_STICK_LEFT:  return GamePad.GetAxis(Axis.R_STICK_X) < -0.5f;
                case Button.R_STICK_RIGHT: return GamePad.GetAxis(Axis.R_STICK_X) > 0.5f;
            }
            return false;
        }

        public static float GetAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.DPAD_X:
                    return Input.GetAxis("DPad_XAxis");
                case Axis.DPAD_Y:
                    return -Input.GetAxis("DPad_YAxis");
                case Axis.L_STICK_X:
                    return Input.GetAxis("L_XAxis");
                case Axis.L_STICK_Y:
                    return -Input.GetAxis("L_YAxis");
                case Axis.R_STICK_X:
                    return Input.GetAxis("R_XAxis");
                case Axis.R_STICK_Y:
                    return -Input.GetAxis("R_YAxis");
                case Axis.TRIGGERS:
                    float LTrigger = Input.GetAxis("TriggersL");
                    float RTrigger = Input.GetAxis("TriggersR");
                    return RTrigger - LTrigger;
            }
            return 0;
        }

        public static Button GetButton(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.JoystickButton0: return Button.A; 
                case KeyCode.JoystickButton1: return Button.B;
                case KeyCode.JoystickButton2: return Button.X;
                case KeyCode.JoystickButton3: return Button.Y;
                case KeyCode.JoystickButton5: return Button.R_SHOULDER;
                case KeyCode.JoystickButton4: return Button.L_SHOULDER;
                case KeyCode.JoystickButton6: return Button.BACK;
                case KeyCode.JoystickButton7: return Button.START;
                case KeyCode.JoystickButton8: return Button.L_STICK;
                case KeyCode.JoystickButton9: return Button.R_STICK;
                case KeyCode.Joystick1Button0: return Button.A;         // only the first joystick will be recognized as one during keybinding
                case KeyCode.Joystick1Button1: return Button.B;
                case KeyCode.Joystick1Button2: return Button.X;
                case KeyCode.Joystick1Button3: return Button.Y;
                case KeyCode.Joystick1Button5: return Button.R_SHOULDER;
                case KeyCode.Joystick1Button4: return Button.L_SHOULDER;
                case KeyCode.Joystick1Button6: return Button.BACK;
                case KeyCode.Joystick1Button7: return Button.START;
                case KeyCode.Joystick1Button8: return Button.L_STICK;
                case KeyCode.Joystick1Button9: return Button.R_STICK;
            }
            return Button.NONE;
        }
    }
}