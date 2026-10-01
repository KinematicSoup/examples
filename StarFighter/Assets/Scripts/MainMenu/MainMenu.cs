using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour 
{
    private const float MAX_SHADOW_DISTANCE = 150f;

    public Canvas Canvas;
    public RectTransform MainMenuPanel;
    
    public RectTransform SettingsMenu;
    public DiscreteSlider SliderResolution;
    public Toggle ToggleAntialiasing;
    public Toggle ToggleBloom;
    public Toggle ToggleShadows;
    public DiscreteSlider SliderShadow;
    public DiscreteSlider SliderVolume;
    public DiscreteSlider SliderMusic;
    public Toggle TogglePrediction;
    
    public RectTransform ControlsMenu;
    public DiscreteSlider SliderMouse;
    public DiscreteSlider SliderTurnDeadzone;
    public Toggle ToggleInvertX;
    public Toggle ToggleInvertY;
    public Button ButtonBindings;
    
    public RectTransform BindingsMenu;
    public RectTransform RectBindings;
    public RectTransform PanelBindingsPrefab;
    public Scrollbar ScrollbarBindings;
    public Button ButtonReloadDefaults;

    public RectTransform RebindMenu;
    public Button ButtonRebind;
    public Text TextRebind;

    public RectTransform RebindDialogue;
        
    public RectTransform CustomizationMenu;
    public InputField InputName;
    public Slider SliderHue;
    public Slider SliderSaturation;
    public Slider SliderBrightness;

    public float MenuSwitchSpeed = 1.0f;
    public Image FadeBlack;
    public float FadeBlackSpeed = 1.0f;
    public AudioSource Music;
    public Transform Fighter;
    public Transform Turret;
    public float FighterRotateSpeed = 1.0f;

    private Vector2 m_activeMenuPos;
	private Vector2 m_inactiveMainMenuPos;
    private Vector2 m_inactiveMenuPos;
    private Vector3 m_inactiveFighterPos;
    private Vector3 m_activeFighterPos;

    private Vector2 m_mainMenuVelocity = Vector2.zero;
    private Vector2 m_settingsMenuVelocity = Vector2.zero;
    private Vector2 m_controlsMenuVelocity = Vector2.zero;
    private Vector2 m_bindingsMenuVelocity = Vector2.zero;
    private Vector2 m_customizationMenuVelocity = Vector2.zero;
    private Vector3 m_fighterVelocity = Vector3.zero;

    private enum Menu { MAIN, SETTINGS, CONTROLS, BINDINGS, CUSTOMIZATION, NONE};
    private Menu m_activeMenu = Menu.MAIN;

    private InputButton m_assignButton; // the control we are modifying in the bindings menu
    private InputAxis m_assignAxis;

    private void Start()
    {
        Settings.LoadSettings();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        CalculateMenuPositions();
        SetPosition(MainMenuPanel, m_inactiveMainMenuPos);
        SetPosition(SettingsMenu, m_inactiveMenuPos);
        SetPosition(ControlsMenu, m_inactiveMenuPos);
        SetPosition(BindingsMenu, m_inactiveMenuPos);
        SetPosition(CustomizationMenu, m_inactiveMenuPos);

        RebindMenu.gameObject.SetActive(false);
        RebindDialogue.gameObject.SetActive(false);

        m_inactiveFighterPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.265f, -Screen.height / 3, 0.6f));
        m_activeFighterPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.265f, Screen.height / 2, 0.6f));
        m_inactiveFighterPos = Fighter.transform.parent.worldToLocalMatrix * m_inactiveFighterPos;
        m_activeFighterPos = Fighter.transform.parent.worldToLocalMatrix * m_activeFighterPos;
        Fighter.position = m_inactiveFighterPos;

        SliderResolution.OnChange += (DiscreteSlider slider) => Settings.ResolutionScale = slider.Value;
        SliderShadow.OnChange += (DiscreteSlider slider) => Settings.ShadowDistance = slider.Value * MAX_SHADOW_DISTANCE;
        SliderVolume.OnChange += (DiscreteSlider slider) => Settings.Volume = slider.Value;
        SliderMusic.OnChange += (DiscreteSlider slider) => Settings.MusicVolume = slider.Value;
        TogglePrediction.onValueChanged.AddListener((bool value) => Settings.UsePrediction = value);

        SliderMouse.OnChange += (DiscreteSlider slider) => Settings.MouseSensitivity = slider.Value;
        SliderTurnDeadzone.OnChange += (DiscreteSlider slider) => Settings.TurnDeadzone = slider.Value;
        ToggleInvertX.onValueChanged.AddListener((bool value) => Settings.InvertX = value);
        ToggleInvertY.onValueChanged.AddListener((bool value) => Settings.InvertY = value);
    }

    void Update ()
    {
        Music.volume = Settings.MusicVolume * 0.5f;

        // rotates the fighter around for viewing during customization
        Fighter.Rotate(0, FighterRotateSpeed * Time.deltaTime, 0);

        Fighter.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", Settings.FighterGlow);

        // loads a chosen level
        if (LevelManager.IsLoading() && m_activeMenu != Menu.MAIN)
        {
            if (LevelManager.GetLoadProgress() >= 0.9f)
            {
                FadeBlack.color = Color.Lerp(FadeBlack.color, new Color(0f, 0f, 0f, 1f), Time.deltaTime * FadeBlackSpeed);
                AudioListener.volume = Mathf.Lerp(AudioListener.volume, 0, Time.deltaTime * FadeBlackSpeed);

                if (FadeBlack.color.a > 0.495f)
                {
                    LevelManager.AllowActivation();
                }
            }
        }
        else
        {
            AudioListener.volume = Settings.Volume;
        }

        // moves menus
        CalculateMenuPositions();

        Vector2 mainMenuTarget              = (m_activeMenu == Menu.MAIN ? m_activeMenuPos : m_inactiveMainMenuPos);
        Vector2 settingsMenuTarget          = (m_activeMenu == Menu.SETTINGS ? m_activeMenuPos : m_inactiveMenuPos);
        Vector2 controlsMenuTarget          = (m_activeMenu == Menu.CONTROLS || m_activeMenu == Menu.BINDINGS ? m_activeMenuPos : m_inactiveMenuPos);
        Vector2 bindingsMenuTarget          = (m_activeMenu == Menu.BINDINGS ? m_activeMenuPos : m_inactiveMenuPos);
        Vector2 customizationMenuTarget     = (m_activeMenu == Menu.CUSTOMIZATION ? m_activeMenuPos : m_inactiveMenuPos);
        Vector3 fighterTarget               = (m_activeMenu == Menu.CUSTOMIZATION ? m_activeFighterPos : m_inactiveFighterPos);
        
        SetPosition(MainMenuPanel,       Vector2.SmoothDamp(GetPosition(MainMenuPanel),       mainMenuTarget,             ref m_mainMenuVelocity,           1.0f / MenuSwitchSpeed, float.MaxValue, Time.deltaTime));
        SetPosition(SettingsMenu,        Vector2.SmoothDamp(GetPosition(SettingsMenu),        settingsMenuTarget,         ref m_settingsMenuVelocity,       1.0f / MenuSwitchSpeed, float.MaxValue, Time.deltaTime));
        SetPosition(ControlsMenu,        Vector2.SmoothDamp(GetPosition(ControlsMenu),        controlsMenuTarget,         ref m_controlsMenuVelocity,       1.0f / MenuSwitchSpeed, float.MaxValue, Time.deltaTime));
        SetPosition(BindingsMenu,        Vector2.SmoothDamp(GetPosition(BindingsMenu),        bindingsMenuTarget,         ref m_bindingsMenuVelocity,       1.0f / MenuSwitchSpeed, float.MaxValue, Time.deltaTime));
        SetPosition(CustomizationMenu,   Vector2.SmoothDamp(GetPosition(CustomizationMenu),   customizationMenuTarget,    ref m_customizationMenuVelocity,  1.0f / MenuSwitchSpeed, float.MaxValue, Time.deltaTime));

        Fighter.localPosition = Vector3.SmoothDamp(Fighter.localPosition, fighterTarget, ref m_fighterVelocity, 1.0f / MenuSwitchSpeed, float.MaxValue, Time.deltaTime);

        // other input
        if (!Controls.IsRebinding() && !RebindDialogue.gameObject.activeSelf && !LevelManager.IsLoading() && (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel")))
        {
            GameObject selected_go = EventSystem.current.currentSelectedGameObject;
            if (selected_go && selected_go.GetComponent<InputField>() && selected_go.GetComponent<InputField>().isFocused)
            {
                selected_go.GetComponent<InputField>().DeactivateInputField();
            }
            else if (Input.GetButtonDown("Cancel"))
            {
                ToMainMenu();
            }
        }

        // if we are done rebinding a control, reset
        if (!Controls.IsRebinding() && RebindDialogue.gameObject.activeSelf && (m_assignButton != null || m_assignAxis != null))
        {
            m_assignButton = null;
            m_assignAxis = null;
            RefreshBindings();

            StartCoroutine(RenableNavigation());
        }

        // if we are rebinding a key show the dialogue
        RebindDialogue.gameObject.SetActive(Controls.IsRebinding());
    }


    // ----------- UI EVENTS ------------
    public void ButtonPlay()
    {
        LevelManager.LoadAsync(LevelManager.Level.ARENA);
        m_activeMenu = Menu.NONE;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        GetComponent<AudioSource>().Play();
    }

    public void Button_Settings()
    {
        Settings.LoadSettings();

        SliderResolution.Value      = Settings.ResolutionScale;
        ToggleAntialiasing.isOn    = Settings.UseAntialiasing;
        ToggleBloom.isOn           = Settings.UseBloom;
        ToggleShadows.isOn         = Settings.UseShadows;
        SliderShadow.Value          = Settings.ShadowDistance / MAX_SHADOW_DISTANCE;
        SliderVolume.Value           = Settings.Volume;
        SliderMusic.Value           = Settings.MusicVolume;
        TogglePrediction.isOn       = Settings.UsePrediction;

        m_activeMenu = Menu.SETTINGS;
        EventSystem.current.SetSelectedGameObject(SettingsMenu.Find("Button_Accept").gameObject);
        GetComponent<AudioSource>().Play();
    }

    public void Button_Controls()
    {
        Settings.LoadSettings();

        SliderMouse.Value = Settings.MouseSensitivity;
        SliderTurnDeadzone.Value = Settings.TurnDeadzone;
        ToggleInvertX.isOn = Settings.InvertX;
        ToggleInvertY.isOn = Settings.InvertY;

        m_activeMenu = Menu.CONTROLS;
        EventSystem.current.SetSelectedGameObject(ControlsMenu.Find("Button_Accept").gameObject);
        GetComponent<AudioSource>().Play();
    }

    public void Button_Customization()
    {
        Settings.LoadSettings();

        float h, s, v;
        RGBToHSV(Settings.FighterGlow, out h, out s, out v);

        InputName.text = Settings.PlayerName;
        SliderHue.value = h;
        SliderSaturation.value = s;
        SliderBrightness.value = v;

        m_activeMenu = Menu.CUSTOMIZATION;
        EventSystem.current.SetSelectedGameObject(CustomizationMenu.Find("Button_Accept").gameObject);
        GetComponent<AudioSource>().Play();
    }

    public void Button_Quit()
    {
        Application.Quit();
    }

    public void Button_Bindings()
    {
        Controls.LoadControls();
        RefreshBindings();

        m_activeMenu = Menu.BINDINGS;
        EventSystem.current.SetSelectedGameObject(BindingsMenu.Find("Button_AcceptBindings").gameObject);
        GetComponent<AudioSource>().Play();
    }
    public void Button_ReloadDefaults()
    {
        Controls.loadDefaultControls();
        RefreshBindings();
    }
    public void Button_AcceptBindings()
    {
        Controls.SaveControls();

        m_activeMenu = Menu.CONTROLS;
        EventSystem.current.SetSelectedGameObject(ControlsMenu.Find("Button_Bindings").gameObject);
        GetComponent<AudioSource>().Play();
    }


    public void Button_Rebind()
    {
        if (m_assignButton != null)
        {
            Controls.StartRebind(m_assignButton, 30);
        }
        else if (m_assignAxis != null)
        {
            Controls.StartRebind(m_assignAxis, 30);
        }

        EventSystem.current.sendNavigationEvents = false;
        RebindMenu.gameObject.SetActive(false);
        RebindDialogue.gameObject.SetActive(true);
    }
    public void Button_RebindClear()
    {
        if (m_assignButton != null)
        {
            Controls.ClearControl(m_assignButton);
        }
        else if (m_assignAxis != null)
        {
            Controls.ClearControl(m_assignAxis);
        }

        m_assignButton = null;
        m_assignAxis = null;
        RefreshBindings();

        RebindMenu.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(BindingsMenu.Find("Button_AcceptBindings").gameObject);
    }
    public void Button_RebindCancel()
    {
        RebindMenu.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(BindingsMenu.Find("Button_AcceptBindings").gameObject);
    }


    public void Button_Accept()
    {
        Settings.SaveSettings();
        ToMainMenu();
    }

    public void Button_Cancel()
    {
        ToMainMenu();
    }

    public void Toggle_Antialiasing(bool val)
    {
        Settings.UseAntialiasing = val;
    }
    public void Toggle_Bloom(bool val)
    {
        Settings.UseBloom = val;
    }
    public void Toggle_Shadows(bool val)
    {
        Settings.UseShadows = val;
    }


    public void Toggle_InvertedControls(bool val)
    {
        Settings.InvertY = val;
    }

    public void Field_Name(string val)
    {
        Settings.PlayerName = val;
    }
    public void Slider_GlowColor()
    {
        Settings.FighterGlow = HSVToRGB(SliderHue.value, SliderSaturation.value, SliderBrightness.value);
    }


    //---------- Other Functions -----------

    /*
     * Renables navigation input a frame after it is called.
     */
    private IEnumerator RenableNavigation()
    {
        yield return 0;

        EventSystem.current.sendNavigationEvents = true;
        EventSystem.current.SetSelectedGameObject(BindingsMenu.Find("Button_AcceptBindings").gameObject);
    }

    /*
     * Loads in the current control bindings and displays them.
     */
    private void RefreshBindings()
    {
        // removes any existing control binding ui elements
        foreach (Transform child in RectBindings)
        {
            Destroy(child.gameObject);
        }

        // adds the ui elements for the different controls 
        foreach (InputButton button in Controls.Buttons)
        {
            RectTransform bindingsPanel = Instantiate(PanelBindingsPrefab);
            bindingsPanel.SetParent(RectBindings, false);
            bindingsPanel.localScale = Vector3.one;
            bindingsPanel.GetComponent<BindingsPanel>().Initialize(button);
        }
        foreach (InputAxis axis in Controls.Axis)
        {
            RectTransform bindingsPanel = Instantiate(PanelBindingsPrefab);
            bindingsPanel.SetParent(RectBindings, false);
            bindingsPanel.localScale = Vector3.one;
            bindingsPanel.GetComponent<BindingsPanel>().Initialize(axis);
        }

        // sets up the navagation between the newly created ui elements
        for (int i = 0; i < RectBindings.childCount; i++)
        {
            BindingsPanel panelAbove = null, panelBelow = null;

            if (i > 0)
            {
                panelAbove = RectBindings.GetChild(i - 1).GetComponent<BindingsPanel>();
            }
            if (i < RectBindings.childCount - 1)
            {
                panelBelow = RectBindings.GetChild(i + 1).GetComponent<BindingsPanel>();
            }

            RectBindings.GetChild(i).GetComponent<BindingsPanel>().SetNavigation(panelAbove, panelBelow, ScrollbarBindings, ButtonReloadDefaults, this);
        }

        Navigation scrollbarNav = ScrollbarBindings.navigation;
        scrollbarNav.selectOnLeft = RectBindings.GetChild(RectBindings.childCount - 1).GetComponent<BindingsPanel>().GetJButton();
        ScrollbarBindings.navigation = scrollbarNav;

        Navigation buttonNav = ButtonReloadDefaults.navigation;
        buttonNav.selectOnUp = RectBindings.GetChild(RectBindings.childCount - 1).GetComponent<BindingsPanel>().GetPButton();
        ButtonReloadDefaults.navigation = buttonNav;
    }

    /*
     * If a control binding is selected, bring up the rebind dialogue with the correct description.
     */
    public void OpenRebindMenu(InputButton button, InputAxis axis)
    {
        string command = (button != null) ? button.Name : axis.Name;
        string controls = ((button != null) ? button.PcButton.ToString() : axis.MouseAxis.Name) + " / " + ((button != null) ? button.JoystickButton.Name : axis.JoystickAxis.Name);

        TextRebind.text = "Do you wish to rebind the controls for <color=#ffffffff>" + command + "</color> from <color=#ffffffff>" + controls + "</color> ?";

        m_assignButton = button;
        m_assignAxis = axis;

        RebindMenu.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(ButtonRebind.gameObject);
    }

    /*
     * Returns to the main menu and selects the first button.
     */
    private void ToMainMenu()
    {
        m_activeMenu = Menu.MAIN;
        EventSystem.current.SetSelectedGameObject(MainMenuPanel.Find("Button_Play").gameObject);
        GetComponent<AudioSource>().Play();
    }

    /*
     * Sets the vertical position of a menu. pos contains the top value in x and the bottom value in y.
     */
    private void SetPosition(RectTransform t, Vector2 pos)
    {
        t.offsetMin = new Vector2(t.offsetMin.x, pos.x);
        t.offsetMax = new Vector2(t.offsetMax.x, pos.y);
    }

    /*
     * Returns a Vector2 containing the top value in x and the bottom value in y of the RectTransform.
     */
    private Vector2 GetPosition(RectTransform t)
    {
        return new Vector2(t.offsetMin.y, t.offsetMax.y);
    }

    /*
     * Calculates new positions for where the menus go when active/inactive based on screen size.
     */
    private void CalculateMenuPositions()
    {
        float offset = 40;
        m_activeMenuPos = new Vector2(offset, -offset);
        m_inactiveMainMenuPos = new Vector2(offset + (Screen.height / Canvas.scaleFactor), (Screen.height / Canvas.scaleFactor) - offset);
        m_inactiveMenuPos = new Vector2(offset - (Screen.height / Canvas.scaleFactor), -(offset + (Screen.height / Canvas.scaleFactor)));
    }

    /*
     * Converts HSV to RGB color space.
     */
    private Color HSVToRGB(float H, float S, float V)
    {
         if (S == 0f)
             return new Color(V,V,V);
         else if (V == 0f)
             return Color.black;
         else
         {
             Color col = Color.black;
             float Hval = H * 6f;
             int sel = Mathf.FloorToInt(Hval);
             float mod = Hval - sel;
             float v1 = V * (1f - S);
             float v2 = V * (1f - S * mod);
             float v3 = V * (1f - S * (1f - mod));
             switch (sel + 1)
             {
             case 0:
                 col.r = V;
                 col.g = v1;
                 col.b = v2;
                 break;
             case 1:
                 col.r = V;
                 col.g = v3;
                 col.b = v1;
                 break;
             case 2:
                 col.r = v2;
                 col.g = V;
                 col.b = v1;
                 break;
             case 3:
                 col.r = v1;
                 col.g = V;
                 col.b = v3;
                 break;
             case 4:
                 col.r = v1;
                 col.g = v2;
                 col.b = V;
                 break;
             case 5:
                 col.r = v3;
                 col.g = v1;
                 col.b = V;
                 break;
             case 6:
                 col.r = V;
                 col.g = v1;
                 col.b = v2;
                 break;
             case 7:
                 col.r = V;
                 col.g = v3;
                 col.b = v1;
                 break;
             }
             col.r = Mathf.Clamp(col.r, 0f, 1f);
             col.g = Mathf.Clamp(col.g, 0f, 1f);
             col.b = Mathf.Clamp(col.b, 0f, 1f);
             return col;
        }
    }

    /*
     * Converts RGB to HSV color space.
     */
    private void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
    {
        if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
        {
            RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
        }
        else
        {
            if (rgbColor.g > rgbColor.r)
            {
                RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
            }
            else
            {
                RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
            }
        }
    }

    private void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
    {
        V = dominantcolor;
        if (V != 0f)
        {
            float num = 0f;
            if (colorone > colortwo)
            {
                num = colortwo;
            }
            else
            {
                num = colorone;
            }
            float num2 = V - num;
            if (num2 != 0f)
            {
                S = num2 / V;
                H = offset + (colorone - colortwo) / num2;
            }
            else
            {
                S = 0f;
                H = offset + (colorone - colortwo);
            }
            H /= 6f;
            if (H < 0f)
            {
                H += 1f;
            }
        }
        else
        {
            S = 0f;
            H = 0f;
        }
    }
}
