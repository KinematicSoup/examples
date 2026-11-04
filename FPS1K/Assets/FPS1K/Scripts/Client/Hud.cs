using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KS.Reactor;
using TMPro;

// Controls the GUI
public class Hud : MonoBehaviour
{
    public static Hud Instance;

    public Color MaxColor;

    public GameObject ConnectScreen;
    public GameObject GameScreen;
    public Button ConnectButton;
    public Button SettingsButton;
    public TMP_InputField UsernameField;
    public TMP_InputField PortField;
    public TMP_Text Username;
    public HealthBar HealthBar;
    public Minimap Minimap;
    public Image RedScreen;
    public Image BlackScreen;
    public TMP_Text Timer;
    public TMP_Text Winner;
    public Image BulletIcon;
    public TMP_Text Bullets;
    public Image GrenadeIcon;
    public TMP_Text Grenades;
    public Image Reticle;
    public Image NoAmmoReticle;
    public TMP_Text FrameRate;
    public TMP_Text Bandwidth;
    public TextOverlayUI TextOverlay;
    public NotificationList Notifications;
    public SettingsUI Settings;

    private bool m_fadeToBlack = false;
    private AmmoTypes m_ammoType = AmmoTypes.NONE;
    private AmmoRow[] m_ammoRows = new AmmoRow[AmmoConsts.NUM_TYPES];

    private struct AmmoRow
    {
        public Image Icon;
        public TMP_Text Text;

        public AmmoRow(Image icon, TMP_Text text)
        {
            Icon = icon;
            Text = text;
        }
    }

    public void Awake()
    {
        Instance = this;
        m_ammoRows[(int)AmmoTypes.BULLETS] = new AmmoRow(BulletIcon, Bullets);
        m_ammoRows[(int)AmmoTypes.GRENADES] = new AmmoRow(GrenadeIcon, Grenades);
        ShowConnectScreen();
        SettingsButton.onClick.AddListener(() => Settings.gameObject.SetActive(true));
    }

    public void Update()
    {
        Color color = RedScreen.color;
        if (color.a > 0f)
        {
            color.a -= 1.5f * Time.deltaTime;
            color.a = Math.Max(0f, color.a);
            RedScreen.color = color;
        }
        if (m_fadeToBlack)
        {
            color = BlackScreen.color;
            if (color.a < 1f)
            {
                color.a += Time.deltaTime;
                color.a = Math.Min(1f, color.a);
                BlackScreen.color = color;
            }
        }
    }

    public void ShowConnectScreen()
    {
        ConnectScreen.SetActive(true);
        GameScreen.SetActive(false);
        m_fadeToBlack = false;
    }

    public void ShowGameScreen()
    {
        ConnectScreen.SetActive(false);
        GameScreen.SetActive(true);
    }

    public void ShowDeathScreen()
    {
        m_fadeToBlack = true;
    }

    public void HideDeathScreen()
    {
        m_fadeToBlack = false;
        BlackScreen.color = new UnityEngine.Color(0, 0, 0, 0);
    }

    public void IndicateDamage()
    {
        Color color = RedScreen.color;
        color.a = .5f;
        RedScreen.color = color;
    }

    public void SetAmmo(AmmoTypes type, int amount, int max)
    {
        if (type == AmmoTypes.NONE)
        {
            return;
        }
        AmmoRow row = m_ammoRows[(int)type];
        row.Text.text = "x " + amount;
        if (amount >= max)
        {
            row.Text.color = MaxColor;
        }
        else
        {
            row.Text.color = amount > 0 ? Color.white : Color.red;
        }
        row.Icon.color = row.Text.color;
        if (type == m_ammoType)
        {
            SetHasAmmo(amount > 0);
        }
    }

    // Sets which ammo type controls the reticle. Will show a different reticle when this ammo type is zero.
    public void SetAmmoType(AmmoTypes type)
    {
        if (type == m_ammoType)
        {
            return;
        }
        m_ammoType = type;
        if (type == AmmoTypes.NONE)
        {
            SetHasAmmo(true);
        }
        else
        {
            SetHasAmmo(m_ammoRows[(int)type].Text.color != Color.red);
        }
    }

    private void SetHasAmmo(bool hasAmmo)
    {
        Reticle.gameObject.SetActive(hasAmmo);
        NoAmmoReticle.gameObject.SetActive(!hasAmmo);
    }

    public void SetTime(float seconds)
    {
        seconds = Mathf.Max(seconds, 0f);
        int minutes = (int)(seconds / 60);
        seconds %= 60;
        if (minutes > 0 || seconds >= 10f)
        {
            if (seconds < 10f)
            {
                Timer.text = minutes + ":0" + (int)seconds;
            }
            else
            {
                Timer.text = minutes + ":" + (int)seconds;
            }
        }
        else
        {
            seconds = Math.Min(seconds, 9.9f);
            Timer.text = "0:0" + seconds.ToString("0.0");
        }
    }
}