using UnityEngine;
using System;
using System.Collections;
using KS.Reactor;

/*
 * stores and maintains user settings
 */
public class Settings : MonoBehaviour 
{
    //---------- Default Values ----------
    private const float  DEFAULT_RESOLUTION_SCALE        = 1.0f;
    private const bool   DEFAULT_USE_ANTIALIASING        = true;
    private const bool   DEFAULT_USE_BLOOM               = true;
    private const bool   DEFAULT_USE_SHADOWS             = true;
    private const float  DEFAULT_SHADOW_DISTNACE         = 50;
    private const float  DEFAULT_VOLUME                  = 1.0f;
    private const float  DEFAULT_MUSIC_VOLUME            = 1.0f;

    private const bool   DEFAULT_USE_DEMO_MODE           = false;
    private const bool   DEFAULT_INVERTED_CONTROLS       = false;
    private const float  DEFAULT_ROLL_SENSITIVITY        = 1.0f;
    private const float  DEFAULT_MOUSE_SENSITIVITY       = 1.0f;
    private const float  DEFAULT_TURN_DEADZONE           = 0.25f;

    private const string DEFAULT_NAME                    = "default";
    private const bool   DEFAULT_SHOW_LEAD_INDICATOR     = true;
    private const bool   DEFAULT_USE_PREDICTION          = true;
    private const float  DEFAULT_FIGHTER_GLOW_R          = 1.0f;
    private const float  DEFAULT_FIGHTER_GLOW_G          = 0.435f;
    private const float  DEFAULT_FIGHTER_GLOW_B          = 0.0f;

    //---------- Settings ----------
    private static float m_resolutionScale;     // the ratio of set resolution to full resolution    
    public static float ResolutionScale
    { 
        get {return m_resolutionScale;} 
        set {m_resolutionScale = value;} 
    }

    private static bool m_useAntialiasing;      // activates FXAA
    public static bool UseAntialiasing
    { 
        get {return m_useAntialiasing;} 
        set {m_useAntialiasing = value;} 
    }

    private static bool m_useBloom;
    public static bool UseBloom
    { 
        get {return m_useBloom;} 
        set {m_useBloom = value;} 
    }

    private static bool m_useShadows;
    public static bool UseShadows
    { 
        get {return m_useShadows;} 
        set {m_useShadows = value;} 
    }

    private static float m_shadowDistance;
    public static float ShadowDistance
    { 
        get {return m_shadowDistance;} 
        set {m_shadowDistance = value;} 
    }

    private static float m_volume;
    public static float Volume
    { 
        get {return m_volume;} 
        set {m_volume = value;} 
    }

    private static float m_musicVolume;
    public static float MusicVolume
    { 
        get {return m_musicVolume;} 
        set {m_musicVolume = value;} 
    }


    //---------- Controls ----------
    private static bool m_useDemoMode;             // uses the simplified controls on mobile devices

    private static bool m_invertX;                  // Invert x-axis input
    public static bool InvertX
    {
        get { return m_invertX; }
        set { m_invertX = value; }
    }

    private static bool m_invertY;                  // Invert y-axis input
    public static bool InvertY
    {
        get { return m_invertY; }
        set { m_invertY = value; }
    }

    private static float m_rollSensitivity;        // ie whether (-15, 15) degrees or (-30, 30) degrees will normalize to (-1, 1)
    public static float RollSensitivity
    {
        get { return m_rollSensitivity; }
        set { m_rollSensitivity = value; }
    }

    private static float m_mouseSensitivity;
    public static float MouseSensitivity
    {
        get { return m_mouseSensitivity; }
        set { m_mouseSensitivity = value; }
    }

    private static float m_turnDeadzone;
    public static float TurnDeadzone
    {
        get { return m_turnDeadzone; }
        set { m_turnDeadzone = value; }
    }
        
    //---------- Customization ----------
    private static string m_playerName;            // the player's displayed name
    public static string PlayerName
    {
        get { return m_playerName; }
        set { m_playerName = value; }
    }

    private static bool m_showLeadIndicator;
    public static bool ShowLeadIndicator
    {
        get { return m_showLeadIndicator; }
        set { m_showLeadIndicator = value; }
    }

    private static bool m_usePrediction;
    public static bool UsePrediction
    {
        get { return m_usePrediction; }
        set { m_usePrediction = value; }
    }

    private static Color m_fighterGlow;           // the player fighter's glow color
    public static Color FighterGlow
    {
        get { return m_fighterGlow; }
        set { m_fighterGlow = value; }
    }

    //---------- Other ----------
    private static int m_defaultResWidth;
    public static int DefaultResWidth
    {
        get { return m_defaultResWidth; }
    }

    private static int m_defaultResHeight;
    public static int DefaultResHeight
    {
        get { return m_defaultResHeight; }
    }

    /*
     * In the main game, find the components affected by the settigs and apply the settings to them on load
     */
	void Awake ()
    {
        LoadSettings();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

		m_defaultResWidth = Screen.width;
        m_defaultResHeight = Screen.height;

        ApplySettings();
	}

    /*
     * Applies changes for the main game
     */
    private void ApplySettings()
    {
        int xRes = (int)(m_resolutionScale * m_defaultResWidth);
        int yRes = (int)(m_resolutionScale * m_defaultResHeight);
        Screen.SetResolution(xRes, yRes, Screen.fullScreen);

        if (m_useShadows)
        {
            QualitySettings.shadowDistance = m_shadowDistance;
        }
        else
        {
            QualitySettings.shadowDistance = 0;
        }

        AudioListener.volume = m_volume;
    }

    /*
     * Loads the settings with the defaults
     */
    public static void LoadDefaults()
    {
        m_resolutionScale       = DEFAULT_RESOLUTION_SCALE;
        m_useAntialiasing       = DEFAULT_USE_ANTIALIASING;
        m_useBloom              = DEFAULT_USE_BLOOM;
        m_useShadows            = DEFAULT_USE_SHADOWS;
        m_shadowDistance        = DEFAULT_SHADOW_DISTNACE;
        m_volume                = DEFAULT_VOLUME;
        m_musicVolume           = DEFAULT_MUSIC_VOLUME;

        m_useDemoMode           = DEFAULT_USE_DEMO_MODE;
        m_invertY      = DEFAULT_INVERTED_CONTROLS;
        m_rollSensitivity       = DEFAULT_ROLL_SENSITIVITY;
        m_mouseSensitivity      = DEFAULT_MOUSE_SENSITIVITY;
        m_turnDeadzone          = DEFAULT_TURN_DEADZONE;

        m_playerName            = DEFAULT_NAME;
        m_showLeadIndicator     = DEFAULT_SHOW_LEAD_INDICATOR;

        m_fighterGlow = ksColor.FromHSV(
            Utils.Random.NextFloat(),
            Utils.Random.NextFloat(.6f, 1f),
            Utils.Random.NextFloat(.8f, 1f));
    }

    /*
     * Saves settings to PlayerPrefs where they can be reloaded
     */
	public static void SaveSettings()
    {
        PlayerPrefsExtras.SetBool("hasSavedSettings", true);
        PlayerPrefs.SetFloat("resolutionScale", m_resolutionScale);
        PlayerPrefsExtras.SetBool("useAntialiasing", m_useAntialiasing);
        PlayerPrefsExtras.SetBool("useBloom", m_useBloom);
		PlayerPrefsExtras.SetBool ("useShadows", m_useShadows);
		PlayerPrefs.SetFloat ("shadowDistance", m_shadowDistance);
        PlayerPrefs.SetFloat("volume", m_volume);
		PlayerPrefs.SetFloat ("musicVolume", m_musicVolume);

        PlayerPrefsExtras.SetBool("useDemoMode", m_useDemoMode);
        PlayerPrefsExtras.SetBool("invertX", m_invertX);
        PlayerPrefsExtras.SetBool("invertY", m_invertY);
        PlayerPrefs.SetFloat("rollSensitivity", m_rollSensitivity);
        PlayerPrefs.SetFloat("mouseSensitivity", m_mouseSensitivity);
        PlayerPrefs.SetFloat("turnDeadzone", m_turnDeadzone);

        PlayerPrefs.SetString("playerName", m_playerName);
        PlayerPrefsExtras.SetBool("usePrediction", m_usePrediction);
        PlayerPrefsExtras.SetBool("showLeadIndicator", m_showLeadIndicator);
        PlayerPrefs.SetFloat("fighterGlowR", m_fighterGlow.r);
        PlayerPrefs.SetFloat("fighterGlowG", m_fighterGlow.g);
        PlayerPrefs.SetFloat("fighterGlowB", m_fighterGlow.b);
	}

    /*
     * Creates a default settings and loads in user data if they have saved anything
     */
	public static void LoadSettings()
    {
        LoadDefaults();

        if (PlayerPrefs.HasKey("hasSavedSettings"))
        {
            m_resolutionScale       = PlayerPrefs.GetFloat("resolutionScale",       DEFAULT_RESOLUTION_SCALE);
            m_useAntialiasing       = PlayerPrefsExtras.GetBool("useAntialiasing",  DEFAULT_USE_ANTIALIASING);
            m_useBloom              = PlayerPrefsExtras.GetBool("useBloom",         DEFAULT_USE_BLOOM);
            m_useShadows            = PlayerPrefsExtras.GetBool("useShadows",       DEFAULT_USE_SHADOWS);
            m_shadowDistance        = PlayerPrefs.GetFloat("shadowDistance",        DEFAULT_SHADOW_DISTNACE);
            m_volume                = PlayerPrefs.GetFloat("volume",                DEFAULT_VOLUME);
            m_musicVolume           = PlayerPrefs.GetFloat("musicVolume",           DEFAULT_MUSIC_VOLUME);

            m_useDemoMode           = PlayerPrefsExtras.GetBool("useDemoMode",      DEFAULT_USE_DEMO_MODE);
            m_invertX               = PlayerPrefsExtras.GetBool("invertX",          DEFAULT_INVERTED_CONTROLS);
            m_invertY               = PlayerPrefsExtras.GetBool("invertY",          DEFAULT_INVERTED_CONTROLS);
            m_rollSensitivity       = PlayerPrefs.GetFloat("rollSensitivity",       DEFAULT_ROLL_SENSITIVITY);
            m_mouseSensitivity      = PlayerPrefs.GetFloat("mouseSensitivity",      DEFAULT_MOUSE_SENSITIVITY);
            m_turnDeadzone          = PlayerPrefs.GetFloat("turnDeadzone",          DEFAULT_TURN_DEADZONE);

            m_playerName            = PlayerPrefs.GetString("playerName",           DEFAULT_NAME);
            m_usePrediction         = PlayerPrefsExtras.GetBool("usePrediction",    DEFAULT_USE_PREDICTION);
            m_showLeadIndicator     = PlayerPrefsExtras.GetBool("showLeadIndicator",DEFAULT_SHOW_LEAD_INDICATOR);
            m_fighterGlow.r         = PlayerPrefs.GetFloat("fighterGlowR",          DEFAULT_FIGHTER_GLOW_R);
            m_fighterGlow.g         = PlayerPrefs.GetFloat("fighterGlowG",          DEFAULT_FIGHTER_GLOW_G);
            m_fighterGlow.b         = PlayerPrefs.GetFloat("fighterGlowB",          DEFAULT_FIGHTER_GLOW_B);
        }
	}
}