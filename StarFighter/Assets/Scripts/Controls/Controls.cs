 using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using KS.Reactor.Client.Unity;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/*
 * stores and maintains user constrols
 */
public class Controls : MonoBehaviour
{
    private static List<InputButton> m_buttons = new List<InputButton>();  // the current active list of buttons inputs that is tested against
    public static List<InputButton> Buttons
    {
        get { return m_buttons; }
    }

    private static List<InputAxis> m_axis = new List<InputAxis>();  // the current active list of axis inputs that is tested against
    public static List<InputAxis> Axis
    {
        get { return m_axis; }
    }

    private static InputButton m_assignButton;
    private static InputAxis m_assignAxis;
    private static bool m_isRebinding = false;
    private static int m_framesUntilRebind = 0;

    void Start()
    {
        LoadControls();
    }

    void Update()
    {
        if (m_isRebinding)
        {
            TryRebind();
        }
    }

    /*
     * needs to run at the end of every frame to make the ButtonDown and ButtonUp funtions work properly
     */
    void LateUpdate()
    {
        foreach (InputButton button in m_buttons)
        {
            button.Update();
        }
    }

    /*
     * clears the current controls and replaces them with the default set
     */
    public static void loadDefaultControls()
    {
        m_buttons.Clear();
        m_axis.Clear();

        m_buttons.Add(new InputButton(GameButton.MENU,              "Back to Menu",         Key.Escape,             InputButton.MouseBinding.NONE,   GamePad.Button.START));
        m_buttons.Add(new InputButton(GameButton.SCORES,            "Show Scores",          Key.Tab,                InputButton.MouseBinding.NONE,   GamePad.Button.BACK));
        m_buttons.Add(new InputButton(GameButton.CHAT,              "Chat",                 Key.Enter,              InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.CLOSE_CHAT,         "Close Chat",           Key.RightShift,         InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.UNLOCK_CURSOR,      "Free Cursor",          Key.LeftCtrl,           InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.PRIMARY_FIRE,       "Primary Fire",         Key.None,               InputButton.MouseBinding.LEFT,   GamePad.Button.R_TRIGGER));
        m_buttons.Add(new InputButton(GameButton.SECONDARY_FIRE,     "Secondary Fire",       Key.None,               InputButton.MouseBinding.RIGHT,  GamePad.Button.L_TRIGGER));
        m_buttons.Add(new InputButton(GameButton.SHIELD,            "Toggle Shield",        Key.LeftShift,          InputButton.MouseBinding.NONE,   GamePad.Button.A));
        m_buttons.Add(new InputButton(GameButton.FORWARD,           "Forward",              Key.W,                  InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.REVERSE,           "Reverse",              Key.S,                  InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.ROLL_LEFT,          "Roll Left",            Key.Q,                  InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.ROLL_RIGHT,         "Roll Right",           Key.E,                  InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.STRAFE_LEFT,        "Strafe Left",          Key.A,                  InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.STRAFE_RIGHT,       "Strafe Right",         Key.D,                  InputButton.MouseBinding.NONE,   GamePad.Button.NONE));
        m_buttons.Add(new InputButton(GameButton.BOOST,             "Speed Boost",          Key.Space,              InputButton.MouseBinding.NONE,   GamePad.Button.L_STICK));
        m_buttons.Add(new InputButton(GameButton.LOOK_BACK,          "Look Behind",          Key.X,                  InputButton.MouseBinding.NONE,   GamePad.Button.L_SHOULDER));
        m_buttons.Add(new InputButton(GameButton.TURRET_AIM,         "Turret Zoom Toggle",   Key.W,                  InputButton.MouseBinding.NONE,   GamePad.Button.DPAD_UP));

        m_axis.Add(new InputAxis(GameAxis.YAW,          "Turn Left/Right",      InputAxis.Mouse.Axis.MOUSE_X,       GamePad.Axis.R_STICK_X));
        m_axis.Add(new InputAxis(GameAxis.PITCH,        "Turn Up/Down",         InputAxis.Mouse.Axis.MOUSE_Y,       GamePad.Axis.R_STICK_Y));
        m_axis.Add(new InputAxis(GameAxis.ROLL,         "Roll Left/Right",      InputAxis.Mouse.Axis.NONE,          GamePad.Axis.NONE));
        m_axis.Add(new InputAxis(GameAxis.STRAFE,       "Strafe Left/Right",    InputAxis.Mouse.Axis.NONE,          GamePad.Axis.NONE));
        m_axis.Add(new InputAxis(GameAxis.ACCELERATE,   "Accelerate",           InputAxis.Mouse.Axis.NONE,          GamePad.Axis.NONE));
        m_axis.Add(new InputAxis(GameAxis.TURRET_X,      "Turret Left/Right",    InputAxis.Mouse.Axis.MOUSE_X,       GamePad.Axis.R_STICK_X));
        m_axis.Add(new InputAxis(GameAxis.TURRET_Y,      "Turret Up/Down",       InputAxis.Mouse.Axis.MOUSE_Y,       GamePad.Axis.R_STICK_Y));
        m_axis.Add(new InputAxis(GameAxis.TURRET_ZOOM,   "Turret Zoom In/Out",   InputAxis.Mouse.Axis.SCROLL_WHEEL,  GamePad.Axis.L_STICK_Y));
    }

    /*
     * returns true if any of the relevant keyboard or joystick keys are held down
     */
    public static bool ButtonValue(GameButton key)
    {
        return GetButton(key).ButtonPressed();
    }

    /*
     * returns true on the first frame a relevant keyboard or joystick key is pressed
     */
    public static bool ButtonDown(GameButton key)
    {
        return GetButton(key).ButtonDown();
    }

    /*
     * returns true when any relevant keyboard or joystick key is released
     */
    public static bool ButtonUp(GameButton key)
    {
        return GetButton(key).ButtonUp();
    }

    /*
     * returns the inputButton containing keyboard and joystick controls for a specific game command
     */
    private static InputButton GetButton(GameButton gameButton)
    {
        foreach (InputButton button in m_buttons)
        {
            if (button.GameButton == gameButton)
            {
                return button;
            }
        }
        return null;
    }

    /*
     * returns the value of an axis, with the option to apply an exponent to the joystick input
     */
    public static float AxisValue(GameAxis axis, float exponent = 1)
    {
        return GetAxis(axis).GetValue(exponent);
    }

    /*
     * returns the inputAxis containing mouse and joystick axis for a specific game command
     */
    private static InputAxis GetAxis(GameAxis gameAxis)
    {
        foreach (InputAxis axis in m_axis)
        {
            if (axis.GameAxis == gameAxis)
            {
                return axis;
            }
        }
        return null;
    }

    /*
     * returns true if we are awaiting input to assign to a command
     */
    public static bool IsRebinding()
    {
        return m_isRebinding;
    }

    /*
     * if we are assigning input and input is detected assign it to the correct control
     * we skip the first few frames to ignore the imput that triggered the rebind command
     */
    public void TryRebind()
    {
        if (m_framesUntilRebind > 0)
        {
            m_framesUntilRebind--;
            return;
        }

        if (m_assignButton != null)
        {
            Key key = FindKey();
            if (key != Key.None)
            {
                m_assignButton.PcButton = key;
                m_assignButton.PcMouseButton = InputButton.MouseBinding.NONE;
                m_assignButton = null;
                m_isRebinding = false;
                return;
            }

            InputButton.MouseBinding mouseButton = FindMouseButton();
            if (mouseButton != InputButton.MouseBinding.NONE)
            {
                m_assignButton.PcButton = Key.None;
                m_assignButton.PcMouseButton = mouseButton;
                m_assignButton = null;
                m_isRebinding = false;
                return;
            }
            
            GamePad.Button button = FindJoystickButton();
            if (button != GamePad.Button.NONE)
            {
                m_assignButton.JoystickButton.Button = button;
                m_assignButton = null;
                m_isRebinding = false;
                return;
            }
        } 
        else if (m_assignAxis != null)
        {
            InputAxis.Mouse.Axis mouseAxis = FindMouseAxis();
            if (mouseAxis != InputAxis.Mouse.Axis.NONE)
            {
                m_assignAxis.MouseAxis.SelectedAxis = mouseAxis;
                m_assignAxis = null;
                m_isRebinding = false;
                return;
            }

            GamePad.Axis axis = FindJoystickAxis();
            if (axis != GamePad.Axis.NONE)
            {
                m_assignAxis.JoystickAxis.SelectedAxis = axis;
                m_assignAxis = null;
                m_isRebinding = false;
                return;
            }
        }
    }

    /*
     * awaits the next input command and assigns it to the pc input of the specified button, after waiting some frames
     */
    public static void StartRebind(InputButton button, int framesToWait)
    {
        m_isRebinding = true;
        m_framesUntilRebind = framesToWait;
        m_assignButton = button;
    }

    /*
     * awaits the next input command and assigns it to the pc input of the specified button, after waiting some frames
     */
    public static void StartRebind(InputAxis axis, int framesToWait)
    {
        m_isRebinding = true;
        m_framesUntilRebind = framesToWait;
        m_assignAxis = axis;
    }

    /*
     * awaits the next input command and assigns it to the pc input of the specified button
     */
    public static void ClearControl(InputButton button)
    {
        button.PcButton = Key.None;
        button.PcMouseButton = InputButton.MouseBinding.NONE;
        button.JoystickButton.Button = GamePad.Button.NONE;
    }

    /*
     * awaits the next input command and assigns it to the pc input of the specified button
     */
    public static void ClearControl(InputAxis axis)
    {
        axis.MouseAxis.SelectedAxis = InputAxis.Mouse.Axis.NONE;
        axis.JoystickAxis.SelectedAxis = GamePad.Axis.NONE;
    }

    /*
     * instantly sets a gameButton to a specified control
     */
    public static void AssignControl(GameButton gameButton, Key pcButton, InputButton.MouseBinding pcMouseButton, GamePad.Button joystickButton)
    {
        InputButton button = GetButton(gameButton);
        button.PcButton = pcButton;
        button.PcMouseButton = pcMouseButton;
        button.JoystickButton.Button = joystickButton;
    }

    /*
     * instantly sets a gameButton to a specified control
     */
    public static void AssignControl(GameAxis gameAxis, InputAxis.Mouse.Axis mouseAxis, GamePad.Axis joystickAxis)
    {
        InputAxis axis = GetAxis(gameAxis);
        axis.MouseAxis.SelectedAxis = mouseAxis;
        axis.JoystickAxis.SelectedAxis = joystickAxis;
    }

    /*
     * returns the first active keyboard key
     * returns None if nothing is found
     */
    private static Key FindKey()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            foreach (KeyControl control in keyboard.allKeys)
            {
                if (!control.wasPressedThisFrame)
                {
                    continue;
                }
                return control.keyCode;
            }
        }

        return Key.None;
    }

    private static InputButton.MouseBinding FindMouseButton()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return InputButton.MouseBinding.NONE;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            return InputButton.MouseBinding.LEFT;
        }
        if (mouse.rightButton.wasPressedThisFrame)
        {
            return InputButton.MouseBinding.RIGHT;
        }
        if (mouse.middleButton.wasPressedThisFrame)
        {
            return InputButton.MouseBinding.MIDDLE;
        }
        if (mouse.forwardButton.wasPressedThisFrame)
        {
            return InputButton.MouseBinding.FORWARD;
        }
        if (mouse.backButton.wasPressedThisFrame)
        {
            return InputButton.MouseBinding.BACK;
        }

        return InputButton.MouseBinding.NONE;
    }

    /*
     * returns the first active joystick button
     * returns None if nothing is found
     */
    private static GamePad.Button FindJoystickButton()
    {
        foreach (GamePad.Button button in Enum.GetValues(typeof(GamePad.Button)))
        {
            if (GamePad.GetButton(button))
            {
                return button;
            }
        }
        return GamePad.Button.NONE;
    }

    /*
     * returns the first active joystick axis
     * returns None if nothing is found
     */
    private static GamePad.Axis FindJoystickAxis()
    {
        foreach (GamePad.Axis axis in Enum.GetValues(typeof(GamePad.Axis)))
        {
            if (Mathf.Abs(GamePad.GetAxis(axis)) > 0.1f)
            {
                return axis;
            }
        }
        return GamePad.Axis.NONE;
    }

    /*
     * returns the first active mouse axis
     * returns None if nothing is found
     */
    private static InputAxis.Mouse.Axis FindMouseAxis()
    {
        foreach (InputAxis.Mouse.Axis axis in Enum.GetValues(typeof(InputAxis.Mouse.Axis)))
        {
            if (Mathf.Abs(InputAxis.Mouse.GetAxis(axis)) > 0.1f)
            {
                return axis;
            }
        }
        return InputAxis.Mouse.Axis.NONE;
    }

    /*
     * Saves the current controls configuration to PlayerPrefs
     */
    public static void SaveControls()
    {
        foreach (InputButton button in Controls.Buttons)
        {
            PlayerPrefs.SetString(button.GameButton.ToString() + "PCKey", button.PcButton.ToString());
            PlayerPrefs.SetString(button.GameButton.ToString() + "PCMouse", button.PcMouseButton.ToString());
            PlayerPrefs.SetString(button.GameButton.ToString() + "Joystick", button.JoystickButton.Button.ToString());
        }
        foreach (InputAxis axis in Controls.Axis)
        {
            PlayerPrefs.SetString(axis.GameAxis.ToString() + "PC", axis.MouseAxis.SelectedAxis.ToString());
            PlayerPrefs.SetString(axis.GameAxis.ToString() + "Joystick", axis.JoystickAxis.SelectedAxis.ToString());
        }
    }

    /*
     * Loads the current controls configuration from PlayerPrefs
     */
    public static void LoadControls()
    {
        loadDefaultControls();

        foreach (GameButton gameButton in Enum.GetValues(typeof(GameButton)))
        {
            string pcKey = PlayerPrefs.GetString(gameButton.ToString() + "PCKey", "");
            string pcMouse = PlayerPrefs.GetString(gameButton.ToString() + "PCMouse", "");
            string joystickButton = PlayerPrefs.GetString(gameButton.ToString() + "Joystick", "");

            if (joystickButton != "" && (pcKey != "" || pcMouse != ""))
            {
                Key pcKeyControl = ParseSavedBinding<Key>(pcKey);
                InputButton.MouseBinding pcMouseControl = ParseSavedBinding<InputButton.MouseBinding>(pcMouse);
                GamePad.Button joystickControl = ParseSavedBinding<GamePad.Button>(joystickButton);

                Controls.AssignControl(gameButton, pcKeyControl, pcMouseControl, joystickControl);
            }
        }
        foreach (GameAxis gameAxis in Enum.GetValues(typeof(GameAxis)))
        {
            string pcAxis = PlayerPrefs.GetString(gameAxis.ToString() + "PC", "");
            string joystickAxis = PlayerPrefs.GetString(gameAxis.ToString() + "Joystick", "");

            if (pcAxis != "" && joystickAxis != "")
            {
                InputAxis.Mouse.Axis pcControl = ParseSavedBinding<InputAxis.Mouse.Axis>(pcAxis);
                GamePad.Axis joystickControl = ParseSavedBinding<GamePad.Axis>(joystickAxis);

                Controls.AssignControl(gameAxis, pcControl, joystickControl);
            }
        }
    }

    private static T ParseSavedBinding<T>(string value) where T : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            return default(T);
        }

        foreach (T candidate in Enum.GetValues(typeof(T)))
        {
            if (string.Equals(candidate.ToString().Replace("_", ""), value.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }
        throw new ArgumentException("Unrecognized control binding: " + value);
    }
}

public enum GameButton
{
    MENU,
    SCORES,
    CHAT,
    CLOSE_CHAT,
    UNLOCK_CURSOR,
    FORWARD,
    REVERSE,
    ROLL_LEFT,
    ROLL_RIGHT,
    STRAFE_LEFT,
    STRAFE_RIGHT,
    PRIMARY_FIRE,
    SECONDARY_FIRE,
    SHIELD,
    BOOST,
    LOOK_BACK,
    TURRET_AIM
}

public enum GameAxis
{
    YAW,
    PITCH,
    ROLL,
    STRAFE,
    ACCELERATE,
    TURRET_X,
    TURRET_Y,
    TURRET_ZOOM,
}
