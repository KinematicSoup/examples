using UnityEngine;
using UnityEngine.InputSystem;
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

        private static Gamepad Current
        {
            get { return Gamepad.current; }
        }

        public static bool GetButton(Button button)
        {
            Gamepad gamepad = Current;
            if (gamepad == null)
            {
                return false;
            }

            switch (button)
            {
                case Button.A:          return gamepad.buttonSouth.isPressed;
                case Button.B:          return gamepad.buttonEast.isPressed;
                case Button.X:          return gamepad.buttonWest.isPressed;
                case Button.Y:          return gamepad.buttonNorth.isPressed;
                case Button.R_SHOULDER: return gamepad.rightShoulder.isPressed;
                case Button.L_SHOULDER: return gamepad.leftShoulder.isPressed;
                case Button.BACK:       return gamepad.selectButton.isPressed;
                case Button.START:      return gamepad.startButton.isPressed;
                case Button.L_STICK:    return gamepad.leftStickButton.isPressed;
                case Button.R_STICK:    return gamepad.rightStickButton.isPressed;

                case Button.L_TRIGGER:   return gamepad.leftTrigger.ReadValue() > 0.3f;
                case Button.R_TRIGGER:   return gamepad.rightTrigger.ReadValue() > 0.3f;
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
            Gamepad gamepad = Current;
            if (gamepad == null)
            {
                return 0f;
            }

            switch (axis)
            {
                case Axis.DPAD_X:
                    return gamepad.dpad.x.ReadValue();
                case Axis.DPAD_Y:
                    return -gamepad.dpad.y.ReadValue();
                case Axis.L_STICK_X:
                    return gamepad.leftStick.x.ReadValue();
                case Axis.L_STICK_Y:
                    return -gamepad.leftStick.y.ReadValue();
                case Axis.R_STICK_X:
                    return gamepad.rightStick.x.ReadValue();
                case Axis.R_STICK_Y:
                    return -gamepad.rightStick.y.ReadValue();
                case Axis.TRIGGERS:
                    float LTrigger = gamepad.leftTrigger.ReadValue();
                    float RTrigger = gamepad.rightTrigger.ReadValue();
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