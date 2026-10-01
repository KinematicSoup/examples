using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using GamepadInput;
using UnityEngine.UI;
using KS.Reactor.Client.Unity;
using KS.Reactor;

public class FighterUI : MonoBehaviour
{
    public Image MissileLock;
    public Button LaserButton;
    public Image LaserReload;
    public Button MissileButton;
    public Image MissileReload;
    public Button BoostButton;
    public Image BoostReload;
    public Button ShieldButton;
    public Image ShieldReload;
    private Image m_crosshair;

    [Tooltip("How fast the missile lock icon scales down per second as the fighter locks")]
    [Range(0, 0.3f)]
    public float TargetLockShrinkSpeed  = 0.2f;

    private ksInputManager m_input;
    private MainUI m_mainUI;
    private ScoreboardUI m_scoreboard;
    private ChatUI m_chat;
    private FighterClient m_fighter;
    private bool m_fireLaser = false;
    private bool m_fireMissile = false;
    private bool m_useShield = false;
    private bool m_boost = false;

    void Start()
    {
        m_mainUI = GetComponent<MainUI>();
        m_scoreboard = GetComponent<ScoreboardUI>();
        m_chat = GetComponent<ChatUI>();
        m_crosshair = m_mainUI.Crosshair;
    }

    void LateUpdate()
    {
        if (PlayerClient.LocalPlayerType != PlayerType.FIGHTER)
        {
            return;
        }
        if (m_fighter == null) 
        {
            GameObject fighter = GameObject.FindGameObjectWithTag("Player");

            if (fighter)
            {
                m_fighter = fighter.GetComponent<FighterClient>();
            }

            m_fireLaser = false;
            m_fireMissile = false;
            m_useShield = false;
        }

        if (m_input == null)
        {
            m_input = ksReactor.InputManager;
        }

        // position the missile lock reticle if the player is locking onto a target, and display it if we are locking and the target is ahead of us
        MissileLock.gameObject.SetActive(false);

        if (m_mainUI.IsTargeting())
        {
            RectTransform lockTransform = MissileLock.GetComponent<RectTransform>();

            Vector3 screenPos = Camera.main.WorldToScreenPoint(m_fighter.GetMissileTarget());
            lockTransform.anchoredPosition = screenPos / GetComponent<Canvas>().scaleFactor;

            lockTransform.localScale = lockTransform.localScale - (lockTransform.localScale.magnitude > 0.35f ? (Vector3.one * TargetLockShrinkSpeed * Time.deltaTime) : Vector3.zero);

            if (screenPos.z > 0)
            {
                MissileLock.gameObject.SetActive(true);
            }
        }
        else
        {
            MissileLock.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        // shows the weapon buttons and other ui elements when applicable
        if (m_fighter != null && !m_mainUI.IsGameOver() && !LevelManager.IsLoading())
        {
            LaserButton.gameObject.SetActive(true);
            MissileButton.gameObject.SetActive(true);
            BoostButton.gameObject.SetActive(true);
            ShieldButton.gameObject.SetActive(true);

            LaserReload.fillAmount = m_fighter.LaserReloadProgress;
            MissileReload.fillAmount = m_fighter.MissileReloadProgress;
            BoostReload.fillAmount = m_fighter.BoostReloadProgress;
            ShieldReload.fillAmount = m_fighter.ShieldReloadProgress;
        }
        else
        {
            LaserButton.gameObject.SetActive(false);
            MissileButton.gameObject.SetActive(false);
            BoostButton.gameObject.SetActive(false);
            ShieldButton.gameObject.SetActive(false);
            MissileLock.gameObject.SetActive(false);
        }

        // ---------- User Input ----------
        Vector2 turnRate = Vector2.zero;
        float roll = 0;
        float accelerate = 0;
        float strafe = 0;

        // accept input if we are not showing the scoreboard or typing in chat
        if (!(m_scoreboard.ShowScores || m_chat.ChatActive))
        {
            // controls
            if (!m_mainUI.IsCursorFree())
            {
                m_fireLaser = Controls.ButtonValue(GameButton.PRIMARY_FIRE);
                m_fireMissile = Controls.ButtonValue(GameButton.SECONDARY_FIRE);
                m_useShield = Controls.ButtonDown(GameButton.SHIELD);
                m_boost = Controls.ButtonValue(GameButton.BOOST);

                accelerate = (Controls.ButtonValue(GameButton.FORWARD) ? 1 : 0) -
                    (Controls.ButtonValue(GameButton.REVERSE) ? 1 : 0) + Controls.AxisValue(GameAxis.ACCELERATE);

                roll = (Controls.ButtonValue(GameButton.ROLL_RIGHT) ? 1 : 0) -
                    (Controls.ButtonValue(GameButton.ROLL_LEFT) ? 1 : 0) + Controls.AxisValue(GameAxis.ROLL);

                strafe = (Controls.ButtonValue(GameButton.STRAFE_RIGHT) ? 1 : 0) -
                    (Controls.ButtonValue(GameButton.STRAFE_LEFT) ? 1 : 0) + Controls.AxisValue(GameAxis.STRAFE);

                turnRate = MoveCrosshair();
            }
        }

        m_input.SetButton(ID.CONTROLS.LASER,    m_fireLaser);
        m_input.SetButton(ID.CONTROLS.MISSILE,  m_fireMissile);
        m_input.SetButton(ID.CONTROLS.SHIELD,   m_useShield);
        m_input.SetButton(ID.CONTROLS.BOOST,    m_boost);

        m_input.SetAxis(ID.CONTROLS.TURN_X,             Mathf.Clamp(turnRate.x, -1, 1));
        m_input.SetAxis(ID.CONTROLS.TURN_Y,             Mathf.Clamp(turnRate.y, -1, 1));
        m_input.SetAxis(ID.CONTROLS.ACCELERATE,         Mathf.Clamp(accelerate, -1, 1));
        m_input.SetAxis(ID.CONTROLS.ROLL,               Mathf.Clamp(roll, -1, 1));
        m_input.SetAxis(ID.CONTROLS.STRAFE,             Mathf.Clamp(strafe, -1, 1));
    }

    private Vector2 MoveCrosshair()
    {
        Vector3 p = m_crosshair.transform.position;
        p.x += 15.0f * Settings.MouseSensitivity * Input.GetAxis("Mouse X") * (Settings.InvertX ? -1f : 1f);
        p.y += 15.0f * Settings.MouseSensitivity * Input.GetAxis("Mouse Y") * (Settings.InvertY ? -1f : 1f);

        float b = Mathf.Min(Screen.height, Screen.width) * 0.3f;
        Rect bounds = new Rect(Screen.width * 0.5f - b, Screen.height * 0.5f - b, 2.0f * b,  2.0f * b);
        p.x = Mathf.Clamp(p.x, bounds.xMin, bounds.xMax);
        p.y = Mathf.Clamp(p.y, bounds.yMin, bounds.yMax);
        m_crosshair.transform.position = p;

        if (m_fighter)
        {
            Ray ray = Camera.main.ScreenPointToRay(m_crosshair.transform.position);
            Vector3 target = ray.GetPoint(1000.0f);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000.0f) && hitInfo.transform != m_fighter.Entity.GameObject.transform)
            {
                target = hitInfo.point;
            }

            m_fighter.SetCrosshairTarget(target);
        }

        float deadZone = Settings.TurnDeadzone * .5f;
        if (deadZone >= 1f)
        {
            return Vector2.zero;
        }
        Vector2 turnRate = new Vector2((p.x - bounds.center.x) / b, (p.y - bounds.center.y) / b);
        Vector2 signs = new Vector2(turnRate.x < 0 ? -1 : 1, turnRate.y < 0 ? -1 : 1);
        if (deadZone > 0f)
        {
            turnRate.x = Mathf.Max(0f, Mathf.Abs(turnRate.x) - deadZone) / (1f - deadZone);
            turnRate.y = Mathf.Max(0f, Mathf.Abs(turnRate.y) - deadZone) / (1f - deadZone);
        }
        turnRate.x = signs.x * turnRate.x * turnRate.x;
        turnRate.y = signs.y * turnRate.y * turnRate.y;
        return turnRate;
    }
}