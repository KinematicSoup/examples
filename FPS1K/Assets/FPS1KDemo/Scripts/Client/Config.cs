using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Persistent user settings
public class Config
{
    private const string USERNAME = "username";
    private const string MOUSE_SENSITIVITY = "mouse_sensitivity";
    private const string INVERT_Y = "invert_y";
    private const string INVERT_X = "invert_x";
    private const string VOLUME = "volume";

    public static Config Instance
    {
        get { return m_instance; }
    }
    private static Config m_instance = new Config();

    public string Username
    {
        get { return PlayerPrefs.GetString(USERNAME, ""); }
        set { PlayerPrefs.SetString(USERNAME, value); }
    }

    public float MouseSensitivity
    {
        get { return PlayerPrefs.GetFloat(MOUSE_SENSITIVITY, 1f); }
        set { PlayerPrefs.SetFloat(MOUSE_SENSITIVITY, value); }
    }
    
    public bool InvertY
    {
        get { return PlayerPrefs.GetInt(INVERT_Y, 0) != 0; }
        set { PlayerPrefs.SetInt(INVERT_Y, value ? 1 : 0); }
    }

    public bool InvertX
    {
        get { return PlayerPrefs.GetInt(INVERT_X, 0) != 0; }
        set { PlayerPrefs.SetInt(INVERT_X, value ? 1 : 0); }
    }

    public float Volume
    {
        get { return PlayerPrefs.GetFloat(VOLUME, 1f); }
        set 
        {
            if (Volume != value)
            {
                PlayerPrefs.SetFloat(VOLUME, value);
                if (OnVolumeChange != null)
                {
                    OnVolumeChange(value);
                }
            }
        }
    }

    public event Action<float> OnVolumeChange;


    public void Save()
    {
        PlayerPrefs.Save();
    }
}
