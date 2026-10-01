using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KS.Reactor.Client.Unity;

/*
 * shows the local player where to aim in order to hit a fighter assuming it travels in the same path
 */
public class FighterLeadIndicator : ksEntityScript
{
    public RectTransform LeadIndicator;

    [Tooltip("The dot product of the the target's displacementa and player's camera direction must be greater then this to fullt display the indicator.")]
    [Range(0.8f, 1)]
    public float DisplayDot = 0.96f;

    [Tooltip("The dot product of the the target's displacementa and player's camera direction must be greater then this to fully hide the indicator.")]
    [Range(0.8f, 1)]
    public float HideDot = 0.92f;

    [Tooltip("How far away from this fighter must be from the camera for the lead indicator to start being visible.")]
    [Range(1, 20)]
    public float BeginFadeInDistance = 5.0f;

    [Tooltip("How far away from this fighter must be from the camera for the lead indicator to be fully visible.")]
    [Range(1, 40)]
    public float EndFadeInDistance = 30.0f;

    [Tooltip("How far away from this fighter must be from the camera for the lead indicator to start fading out.")]
    [Range(1, 200)]
    public float BeginFadeOutDistance = 5.0f;

    [Tooltip("How far away from this fighter must be from the camera for the lead indicator to stop being visible.")]
    [Range(1, 200)]
    public float EndFadeOutDistance = 5.0f;

    [Tooltip("Objects in these layers will block the line of sight raycast.")]
    public LayerMask BlockingLayers;

    [Tooltip("How much the indicator is hidden if the target position travels out of line of sight with the camera.")]
    [Range(0, 1)]
    public float ObscuredAlpha = 0.3f;

    [Tooltip("The default color of the indicator.")]
    public Color BaseColor = Color.white;

    private FighterClient m_fighter;
    private ServerConnection m_connection;
    private GameManagerClient m_gameManager;
    private GameObject m_player;
    private Image m_icon;
    private Canvas m_canvas;
    private Vector3 m_lastPosition;

    /**
     * Called after properties are initialized.
     */
	public override void Initialize()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("GameController");
        foreach (GameObject go in gos)
        {
            m_connection = go.GetComponent<ServerConnection>();
            if (m_connection != null)
            {
                break;
            }
        }
        
        if (!Settings.ShowLeadIndicator)
        {
            Destroy(this);
            return;
        }
        
        m_canvas = GameObject.FindGameObjectWithTag("GUI").GetComponent<Canvas>();

        GameObject playerIndicators = GameObject.FindGameObjectWithTag("LeadIndicators");
        LeadIndicator.SetParent(playerIndicators.transform, false);
        m_lastPosition = transform.position;

        m_icon = LeadIndicator.GetComponent<Image>();
        m_fighter = GetComponent<FighterClient>();
	}

    /**
     * Called when the script is detached.
     */
    public override void Detached()
    {
        if (LeadIndicator)
        {
            Destroy(LeadIndicator.gameObject);
        }
    }

    private void Update()
    {
        if (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        if (m_gameManager == null && m_connection.IsConnected())
        {
            m_gameManager = m_connection.Room.GameObject.GetComponent<GameManagerClient>();
        }
    }

    /**
     * Calculates how to draw the target lead indicator. Finds where the player would need to shoot to hit the target, and displays it. The target is obscured if the fighter
     * is not near the center of the camera view, and is faded if the intercept is behind an object
     */
    private void LateUpdate()
    {
        bool sameTeam = false;
        if (m_fighter != null && m_fighter.TeamNumber >= 0)
        {
            sameTeam = Room.LocalPlayer.GameObject.GetComponent<PlayerClient>().TeamNumber == m_fighter.TeamNumber;
        }

        if (!m_player || sameTeam || m_gameManager != null && m_gameManager.GameOver)
        {
            m_icon.enabled = false;
            return;
        }

        Vector3 velocity = (transform.position - m_lastPosition) / Time.Delta;
        m_lastPosition = transform.position;
        Vector3 intercept = FindIntercept(m_player.transform.position, Properties[ID.PROP.FIGHTER.LASER_SPEED], transform.position, velocity);

        float dot = Vector3.Dot((transform.position - Camera.main.transform.position).normalized, Camera.main.transform.forward);

        // if the bullet has no possible way to reach the target or the target is behind something, hide the indicator
        if (intercept == Vector3.zero || dot < HideDot || Physics.Linecast(transform.position, Camera.main.transform.position, BlockingLayers))
        {
            m_icon.enabled = false;
            return;
        }

        bool obscureIndicator = false;

        if (Physics.Linecast(intercept, Camera.main.transform.position, BlockingLayers))
        {
            obscureIndicator = true;
        }

        if (m_canvas.GetComponent<MainUI>().ShowPlayerIndicators())
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(intercept);

            LeadIndicator.anchoredPosition = screenPos / m_canvas.scaleFactor;

            m_icon.enabled = screenPos.z > 0;

            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            float a = Mathf.Clamp01((dot - HideDot) / (DisplayDot - HideDot));

            if (distance < EndFadeInDistance)
            {
                a *= Mathf.Clamp01((distance - BeginFadeInDistance) / (EndFadeInDistance - BeginFadeInDistance));
            }
            else if (distance > BeginFadeOutDistance)
            {
                a *= Mathf.Clamp01( 1 - ((distance - BeginFadeOutDistance) / (EndFadeOutDistance - BeginFadeOutDistance)) );
            }

            m_icon.color = new Color(BaseColor.r, BaseColor.g, BaseColor.b, BaseColor.a * a * (obscureIndicator ? ObscuredAlpha : 1));
        }
	}

    /*
     * finds where a moving object and a projectile will meet taking only first order movement into account
     */
    private Vector3 FindIntercept(Vector3 shotOrigin, float shotSpeed, Vector3 targetOrigin, Vector3 targetVel)
    {
        Vector3 dirToTarget = Vector3.Normalize(targetOrigin - shotOrigin);

        Vector3 targetVelOrth = Vector3.Dot(targetVel, dirToTarget) * dirToTarget;

        Vector3 targetVelTang = targetVel - targetVelOrth;
        Vector3 shotVelTang = targetVelTang;

        float shotVelSpeed = shotVelTang.magnitude;

        if (shotVelSpeed > shotSpeed)
        {
            return Vector3.zero; // Shot is too slow to intercept target, it will never catch up, so return zero
        }
        else
        {
            float shotSpeedOrth = Mathf.Sqrt(shotSpeed * shotSpeed - shotVelSpeed * shotVelSpeed);
            Vector3 shotVelOrth = dirToTarget * shotSpeedOrth;

            float timeToCollision = ((shotOrigin - targetOrigin).magnitude) / (shotVelOrth.magnitude - targetVelOrth.magnitude);

            Vector3 shotVel = shotVelOrth + shotVelTang;
            return shotOrigin + shotVel * timeToCollision;
        }
    }
}