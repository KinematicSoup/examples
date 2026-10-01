using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Settings menu
public class SettingsUI : MonoBehaviour
{
    // Number of mouse sensitivity steps between 0 and 1.
    public int SensitivityStepsPerOne = 10;
    public int VolumeStepsPerOne = 20;
    public Slider SensitivtySlider;
    public TMP_Text SensitivityValue;
    public Toggle InvertY;
    public Toggle InvertX;
    public Slider VolumeSlider;
    public TMP_Text VolumeValue;
    public Button CloseButton;
    public Button QuitButton;

    public event Action OnClose;

    // Start is called before the first frame update
    private void Start()
    {
        SensitivtySlider.value = Config.Instance.MouseSensitivity * SensitivityStepsPerOne;
        SensitivityValue.text = (Config.Instance.MouseSensitivity * 100f) + "%";
        SensitivtySlider.onValueChanged.AddListener(OnSensitivityChange);
        InvertY.isOn = Config.Instance.InvertY;
        InvertY.onValueChanged.AddListener(OnToggleInvertY);
        InvertX.isOn = Config.Instance.InvertX;
        InvertX.onValueChanged.AddListener(OnToggleInvertX);

        VolumeSlider.value = Config.Instance.Volume * VolumeStepsPerOne;
        VolumeValue.text = (Config.Instance.Volume * 100f) + "%";
        VolumeSlider.onValueChanged.AddListener(OnVolumeChange);

        CloseButton.onClick.AddListener(Close);
        QuitButton.onClick.AddListener(Quit);
    }

    private void OnEnable()
    {
        Hud.Instance.ConnectScreen.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Close the menu if the player clicks off the UI.
        if (Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current != null && 
            !EventSystem.current.IsPointerOverGameObject())
        {
            Close();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (!Hud.Instance.GameScreen.gameObject.activeSelf)
        {
            Hud.Instance.ConnectScreen.gameObject.SetActive(true);
        }
        if (OnClose != null)
        {
            OnClose();
        }
    }

    private void OnSensitivityChange(float value)
    {
        value /= SensitivityStepsPerOne;
        Config.Instance.MouseSensitivity = value;
        Config.Instance.Save();
        SensitivityValue.text = (value * 100f) + "%";
    }

    private void OnVolumeChange(float value)
    {
        value /= VolumeStepsPerOne;
        Config.Instance.Volume = value;
        Config.Instance.Save();
        VolumeValue.text = (value * 100f) + "%";
    }

    private void OnToggleInvertY(bool value)
    {
        Config.Instance.InvertY = value;
        Config.Instance.Save();
    }

    private void OnToggleInvertX(bool value)
    {
        Config.Instance.InvertX = value;
        Config.Instance.Save();
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
