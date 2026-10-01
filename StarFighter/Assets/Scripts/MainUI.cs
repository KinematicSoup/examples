using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Image FadeBlack;
    public GameObject HealthBar;
    public Image HealthBarMeter;
    public GameObject EnergyBar;
    public Image EnergyBarMeter;
    public Image Crosshair;
    public GameObject MissileSlots;
    public GameObject ShieldSlots;
    public Sprite MissileEmpty;
    public Sprite MissileFull;
    public Sprite ShieldEmpty;
    public Sprite ShieldFull;
    public Text CopilotText;
    public Text TeamText;
    public Text RespawnTimeText;
    public Text GameTimeText;
    public Text GameResultText;
    public Text OutOfBoundsText;
    public Text MissileLockText;
    public Button FighterButton;
    public Button TurretButton;

    [Tooltip("How fast the out of bounds warning will blink.")]
    [Range(0, 15)]
    public float OutOfBoundsFlashSpeed  = 5.0f;

    [Tooltip("How fast the incoming missile warning will blink.")]
    [Range(0, 15)]
    public float MissileLabelFlashSpeed = 10.0f;

    [Tooltip("How long it takes as the game's end for the screen to dim and the win/loss text to show.")]
    [Range(0, 5)]
    public float GameResultFadeTime     = 2.0f;

    [Tooltip("How fast the screen fades out when going back to the main menu.")]
    [Range(0, 8)]
    public float FadeBlackSpeed         = 2.0f;

    private ServerConnection m_connection;
    private GameManagerClient m_gameManager;
    private ScoreboardUI m_scoreboard;
    private PlayerClient m_player;
    private GameObject m_fighterObject;

    private float m_gameResultTime = float.MinValue;

    void Start()
    {
        m_connection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ServerConnection>();

        m_scoreboard = GetComponent<ScoreboardUI>();

        FadeBlack.gameObject.SetActive(true);

        FighterButton.onClick.AddListener(SpawnAsFighter);
        TurretButton.onClick.AddListener(SpawnAsTurret);
    }

    void LateUpdate()
    {
        if (m_player == null && m_connection.GetLocalPlayer() != null)
        {
            m_player = m_connection.GetLocalPlayer().GameObject.GetComponent<PlayerClient>();
        }

        if (m_gameManager == null && m_connection.IsConnected())
        {
            m_gameManager = m_connection.Room.GameObject.GetComponent<GameManagerClient>();
        }

        if (m_fighterObject == null) 
        {
            m_fighterObject = GameObject.FindGameObjectWithTag("Player");
        }
        FighterClient fighter = m_fighterObject == null ? null : m_fighterObject.GetComponent<FighterClient>();

        // quits to the menu
        if (Controls.ButtonDown(GameButton.MENU))
        {
            LeaveToMenu();
        }

        // frees the cursor
        Cursor.visible = IsCursorFree() ? true : false;
        Cursor.lockState = IsCursorFree() ? CursorLockMode.None : CursorLockMode.Locked;

        // hides the view until the game is finished loading
        if (m_player != null && !LevelManager.IsLoading())
        {
            FadeBlack.gameObject.SetActive(false);
        }

        // if we are loading a new scene fade out to black, then finish loading it
        if (LevelManager.IsLoading())
        {
            FadeBlack.gameObject.SetActive(true);
            FadeBlack.transform.parent.SetAsLastSibling();
            FadeBlack.color = Color.Lerp(FadeBlack.color, new Color(0, 0, 0, 1.0f), Time.deltaTime * FadeBlackSpeed);
            AudioListener.volume = Mathf.Lerp(AudioListener.volume, 0, Time.deltaTime * FadeBlackSpeed);

            if (FadeBlack.color.a > 0.98f)
            {
                LevelManager.AllowActivation();
                Screen.SetResolution(Settings.DefaultResWidth, Settings.DefaultResHeight, Screen.fullScreen);
            }
        }

        // changes whether the canvas scales with dpi or screen resolution
        CanvasScaler scaler = GetComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;

        // If the fighter is locked by a missile, show indicator and play a sound
        if (!LevelManager.IsLoading() && m_fighterObject && fighter.IsTargeted)
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            }
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }

        // disables ui elements not needed for the current configuration and game state
        TeamText.gameObject.SetActive(false);
        GameResultText.gameObject.SetActive(false);
        GameTimeText.gameObject.SetActive(false);
        HealthBar.gameObject.SetActive(false);
        EnergyBar.gameObject.SetActive(false);
        CopilotText.gameObject.SetActive(false);
        RespawnTimeText.gameObject.SetActive(false);
        OutOfBoundsText.gameObject.SetActive(false);
        MissileLockText.gameObject.SetActive(false);
        bool spawnButtonsActive = false;
        MissileSlots.SetActive(false);
        ShieldSlots.SetActive(false);
        Crosshair.gameObject.SetActive(!IsCursorFree());

        if (!IsGameOver() && !LevelManager.IsLoading())
        {
            if (m_fighterObject)
            {
                HealthBar.SetActive(true);
                EnergyBar.SetActive(true);
                OutOfBoundsText.gameObject.SetActive(fighter.OutOfBounds);
                MissileLockText.gameObject.SetActive(fighter.IsTargeted);

                HealthBarMeter.fillAmount = fighter.Health / fighter.Properties[ID.PROP.FIGHTER.MAX_HEALTH].Float;

                if (PlayerClient.LocalPlayerType == PlayerType.FIGHTER)
                {
                    EnergyBarMeter.fillAmount = fighter.Energy / fighter.Properties[ID.PROP.FIGHTER.MAX_ENERGY].Float;
                    EnergyBarMeter.color = fighter.EnergyFrozen ? new Color(0.5f, 0.5f, 0.5f) : Color.white;


                    // Missiles
                    int i = 0;
                    MissileSlots.SetActive(true);
                    foreach (Transform t in MissileSlots.transform)
                    {
                        if (i < fighter.MissileCapacity)
                        {
                            t.gameObject.SetActive(true);
                            Image img = t.GetComponent<Image>();
                            if (i < fighter.MissileCount && img.sprite != MissileFull)
                            {
                                img.sprite = MissileFull;
                            }
                            else if (i >= fighter.MissileCount && img.sprite != MissileEmpty)
                            {
                                img.sprite = MissileEmpty;
                            }
                        }
                        else
                        {
                            t.gameObject.SetActive(false);
                        }
                        i++;
                    }

                    // Shields
                    i = 0;
                    ShieldSlots.SetActive(true);
                    foreach (Transform t in ShieldSlots.transform)
                    {
                        if (i < fighter.ShieldCapacity)
                        {
                            t.gameObject.SetActive(true);
                            Image img = t.GetComponent<Image>();
                            if (i < fighter.ShieldCount && img.sprite != ShieldFull)
                            {
                                img.sprite = ShieldFull;
                            }
                            else if (i >= fighter.ShieldCount && img.sprite != ShieldEmpty)
                            {
                                img.sprite = ShieldEmpty;
                            }
                        }
                        else
                        {
                            t.gameObject.SetActive(false);
                        }
                        i++;
                    }
                }
                else
                {
                    TurretClient turret = m_fighterObject.GetComponent<TurretClient>();
                    EnergyBarMeter.fillAmount = turret.Energy / turret.Properties[ID.PROP.TURRET.MAX_ENERGY].Float;
                    EnergyBarMeter.color = turret.EnergyFrozen ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
                }

                OutOfBoundsText.color = new Color(1f, 0f, 0f, 0.5f * Mathf.Sin(Time.time * OutOfBoundsFlashSpeed) + 0.5f);
                MissileLockText.color = new Color(1f, 0f, 0f, 0.5f * Mathf.Sin(Time.time * MissileLabelFlashSpeed) + 0.5f);

                string copilotName = GetCopilotName();
                CopilotText.gameObject.SetActive(copilotName != "");
                CopilotText.text = copilotName;
            }
            else if (m_player != null)
            {
                RespawnTimeText.gameObject.SetActive(m_player.RespawnTimeLeft > 0);
                RespawnTimeText.text = m_player.RespawnTimeLeft.ToString();
                if (m_player.RespawnTimeLeft <= 0)
                {
                    spawnButtonsActive = true;
                }
            }
        }

        FighterButton.gameObject.SetActive(spawnButtonsActive);
        TurretButton.gameObject.SetActive(spawnButtonsActive);
        if (!spawnButtonsActive)
        {
            FighterButton.interactable = true;
            if (!TurretButton.interactable)
            {
                TurretButton.interactable = true;
                TurretButton.GetComponentInChildren<Text>().text = "Spawn as Turret";
            }
        }

        if (!LevelManager.IsLoading())
        {
            if (m_player != null && m_player.TeamNumber >= 0)
            {
                TeamText.gameObject.SetActive(true);
                TeamText.text = m_player.TeamName + ": " + m_gameManager.GetTeamRank(m_player.TeamNumber);
                TeamText.color = m_player.Color;
            }

            if (m_gameManager != null && m_gameManager.GameTimer > float.MinValue)
            {
                GameTimeText.gameObject.SetActive(true);
                GameTimeText.text = m_gameManager.GameTimeLeft();
            }
        }

        // if the game is over display whether the player lost or won. Freeze the clock and fade out the background
        if (IsGameOver())
        {
            if (m_gameResultTime == float.MinValue)
            {
                m_gameResultTime = Time.time;
            }

            FadeBlack.gameObject.SetActive(true);
            FadeBlack.color = new Color(0, 0, 0, 0.2f * Mathf.Min((Time.time - m_gameResultTime) / GameResultFadeTime, 1));

            GameResultText.gameObject.SetActive(!m_scoreboard.ShowScores);
            GameResultText.color = new Color(1, 1, 1, Mathf.Min((Time.time - m_gameResultTime) / GameResultFadeTime, 1));

            switch (m_gameManager.MatchResult)
            {
                case GameManagerClient.GameResult.WIN: GameResultText.text = "VICTORY"; break;
                case GameManagerClient.GameResult.LOSS: GameResultText.text = "DEFEAT"; break;
                case GameManagerClient.GameResult.DRAW: GameResultText.text = "DRAW"; break;
            }
        }
        else
        {
            GameResultText.text = "";
        }
    }

    private void SpawnAsFighter()
    {
        m_connection.Room.CallRPC(ID.RPC.SPAWN, (byte)PlayerType.FIGHTER);
        FighterButton.interactable = false;
        TurretButton.interactable = true;
        TurretButton.GetComponentInChildren<Text>().text = "Spawn as Turret";
    }

    private void SpawnAsTurret()
    {
        m_connection.Room.CallRPC(ID.RPC.SPAWN, (byte)PlayerType.TURRET);
        TurretButton.interactable = false;
        FighterButton.interactable = true;
        TurretButton.GetComponentInChildren<Text>().text = "Waiting for Available Fighter...";
        Crosshair.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    /*
     * Gets the name of the player in the same fighter as the local player.
     */
    private string GetCopilotName()
    {
        string name = "";

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (PlayerClient.LocalPlayerType == PlayerType.FIGHTER && player.GetComponent<TurretClient>().IsOccupied)
            {
                return "Gunner: " + player.GetComponent<TurretClient>().PlayerName;
            }
            else if (PlayerClient.LocalPlayerType == PlayerType.TURRET)
            {
                return "Pilot: " + player.GetComponent<FighterClient>().PlayerName;
            }
        }

        return name;
    }

    /*
     * Quits to the menu, smoothly if the game is connected to a server, and instantly if the game is not connected.
     */
    private void LeaveToMenu()
    {
        if (!LevelManager.IsLoading() && m_connection.IsConnected())
        {
            LevelManager.LoadAsync(LevelManager.Level.MAIN_MENU);
            FadeBlack.color = new Color(0, 0, 0, 0);

            m_connection.Disconnect();
        }
        else if (!LevelManager.IsLoading())
        {
            LevelManager.Load(LevelManager.Level.MAIN_MENU);
        }
    }

    public bool IsGameOver()
    {
        return m_gameManager != null && m_gameManager.GameOver && !LevelManager.IsLoading();
    }

    public bool IsTargeting()
    {
        return m_fighterObject && m_fighterObject.GetComponent<FighterClient>().IsTargeting() && !LevelManager.IsLoading();
    }

    public bool IsCursorFree()
    {
        return Controls.ButtonValue(GameButton.UNLOCK_CURSOR) || !m_connection.IsConnected() || m_fighterObject == null;
    }

    public bool ShowPlayerIndicators()
    {
        if (m_gameManager != null)
        {
            return (!m_gameManager.GameOver && !LevelManager.IsLoading());
        }
        return false;
    }
}