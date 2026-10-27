using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class TurretUI : MonoBehaviour
{
    public Button LaserButton;
    public Image LaserReload;

    private MainUI m_mainUI;
    private ScoreboardUI m_scoreboard;
    private ChatUI m_chat;
    private CameraMovement m_cam;
    private GameObject m_fighter;
    private TurretClient m_turret;

    void Start()
    {
        m_mainUI = GetComponent<MainUI>();
        m_scoreboard = GetComponent<ScoreboardUI>();
        m_chat = GetComponent<ChatUI>();
        m_cam = Camera.main.transform.GetComponent<CameraMovement>();
    }

    void LateUpdate()
    {
        if (PlayerClient.LocalPlayerType != PlayerType.TURRET)
        {
            return;
        }
        if (m_fighter == null) 
        {
            m_fighter = GameObject.FindGameObjectWithTag("Player");
            
            if (m_fighter != null)
            {
                m_turret = m_fighter.GetComponent<TurretClient>();
            }

        }

        // shows the weapon buttons when applicable and highlights them is they can be fired
        if (m_fighter && !m_mainUI.IsGameOver() && !LevelManager.IsLoading())
        {
            LaserButton.gameObject.SetActive(true);

            LaserReload.fillAmount = m_fighter.GetComponent<TurretClient>().LaserReloadProgress;
        }
        else
        {
            LaserButton.gameObject.SetActive(false);
        }


        // ---------- User Input ----------

        Vector2 turnRate = Vector2.zero;

        if (!m_scoreboard.ShowScores && !m_chat.ChatActive && m_turret)
        {
            if (!m_mainUI.IsCursorFree())
            {
                if (Controls.ButtonValue(GameButton.PRIMARY_FIRE))
                {
                    m_turret.FireLaser();
                }

                Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
                turnRate.x = mouseDelta.x;
                turnRate.y = mouseDelta.y;
                turnRate *= Settings.MouseSensitivity * m_cam.DesensitizeFactor() / 4f;
            }

            if (Settings.InvertX)
            {
                turnRate.x = -turnRate.x;
            }
            if (Settings.InvertY)
            {
                turnRate.y = -turnRate.y;
            }

            m_turret.ChangeElevation(turnRate.y);
            m_turret.ChangeRotation(turnRate.x);
        }
    }
}