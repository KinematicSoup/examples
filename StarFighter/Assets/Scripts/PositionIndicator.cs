using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine.UI;

/*
 * displays the names of players controlling a fighter and turret, and shows their position beyond a certain distance 
 */
public class PositionIndicator : MonoBehaviour
{
    public RectTransform PositionIcon;
    public RectTransform FighterName;
    public RectTransform TurretName;

    [Tooltip("Will the position indicator stay at the edge of the screen instead of going off.")]
    public bool ClampToScreen = true;

    [Tooltip("How close this fighter must be to the camera for the name to be fully visible.")]
    [Range(1, 200)]
    public float BeginFadeOutNameDistance = 40.0f;

    [Tooltip("How far away this fighter must be from the camera for the position indicator to be fully hidden.")]
    [Range(1, 200)]
    public float EndFadeOutNameDistance = 60.0f;

    [Tooltip("How far away this fighter must be from the camera for the position indicator to start being visible.")]
    [Range(1, 20)]
    public float BeginFadeInDistance = 5.0f;

    [Tooltip("How far away this fighter must be from the camera for the position indicator to be fully visible.")]
    [Range(1, 40)]
    public float EndFadeInDistance = 30.0f;

    [Tooltip("How far away this fighter must be from the camera for the position indicator to be full size.")]
    [Range(1, 30)]
    public float BeginScaleDistance = 12.5f;

    [Tooltip("How far away this fighter must be from the camera for the position indicator to be at mimimal size.")]
    [Range(1, 200)]
    public float EndScaleDistance = 100.0f;

    [Tooltip("The maximum scale of the position indicator, when it is closest to the camera")]
    [Range(0.25f, 4)]
    public float MaxScale = 1.25f;

    [Tooltip("Objects in these layers will block the line of sight raycast.")]
    public LayerMask BlockingLayers;


    private FighterClient m_fighter;
    private TurretClient m_turret;
    private Image m_positionIcon;
    private Text m_fighterName;
    private Text m_turretName;
    public Color Color = Color.grey;
    private Canvas m_canvas;

	void Start ()
    {
        m_canvas = GameObject.FindGameObjectWithTag("GUI").GetComponent<Canvas>();

        GameObject playerIndicators = GameObject.FindGameObjectWithTag("PlayerIndicators");
        PositionIcon.SetParent(playerIndicators.transform, false);

        m_fighter = GetComponent<FighterClient>();
        if (m_fighter != null)
        {
            FighterName.SetParent(playerIndicators.transform, false);
            m_fighterName = FighterName.GetComponent<Text>();
            m_fighterName.text = m_fighter.PlayerName;
        }

        m_turret = GetComponent<TurretClient>();
        if (m_turret != null)
        {
            TurretName.SetParent(playerIndicators.transform, false);
            m_turretName = TurretName.GetComponent<Text>();
        }

        m_positionIcon = PositionIcon.GetComponent<Image>();
	}

    /*
     * draws the position indicator over the fighter or by the edge of the screen in its direction
     */
    void LateUpdate()
    {
        if (m_canvas.GetComponent<MainUI>().ShowPlayerIndicators() && transform.tag != "Player")
        {
            // Position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            float angle = Vector3.Angle(Camera.main.transform.forward, transform.position - Camera.main.transform.position);

            Vector3 halfScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
            screenPos -= halfScreen;
            float radarSize = halfScreen.y * 0.7f;

            if (angle > 21.0f || angle < 21.0f)
            {
                radarSize += (Mathf.Abs(angle) - 12.0f) / 159.0f * halfScreen.y * 0.2f;
            }

            if (screenPos.z > 0.0f)
            {
                if (screenPos.sqrMagnitude > radarSize * radarSize)
                {
                    screenPos = screenPos.normalized * radarSize;
                }
            }
            else
            {
                Vector3 normal = screenPos;
                normal.z = 0;
                normal = normal.normalized * radarSize;
                screenPos.x = -normal.x;
                screenPos.y = -normal.y;
            }
            screenPos += halfScreen;
            PositionIcon.anchoredPosition = screenPos / m_canvas.scaleFactor;
           
            // Scale
            Vector3 scale = Vector3.one * MaxScale * Mathf.Clamp(1 - GetDistanceFactor(BeginScaleDistance, EndScaleDistance), 0.35f, 1.0f);
            if (screenPos.z > 0)
            {
                scale.y = -scale.y;
            }
            PositionIcon.localScale = scale;

            m_positionIcon.color = Color;

            // names
            screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Color nameColor = Color;
            nameColor.a = 1f - GetDistanceFactor(BeginFadeOutNameDistance, EndFadeOutNameDistance);

            if (m_fighter != null)
            {
                FighterName.anchoredPosition = screenPos / m_canvas.scaleFactor;
                m_fighterName.enabled = (screenPos.z > 0);
                m_fighterName.color = nameColor;
            }

            if (m_turret != null)
            {
                TurretName.anchoredPosition = screenPos / m_canvas.scaleFactor;
                m_turretName.enabled = (screenPos.z > 0) && m_turret.IsOccupied;
                m_turretName.text = m_turret.PlayerName;
                m_turretName.color = nameColor;
            }
        }
        else
        {
            m_positionIcon.enabled = false;
            if (m_fighter != null)
            {
                m_fighterName.enabled = false;
            }
            if (m_turret != null)
            {
                m_turretName.enabled = false;
            }
        }
    }

    void OnDestroy()
    {
        if (PositionIcon)
        {
            Destroy(PositionIcon.gameObject);
            if (m_fighter != null)
            {
                Destroy(FighterName.gameObject);
            }
            if (m_turret != null)
            {
                Destroy(TurretName.gameObject);
            }
        }
    }


    /*
     * Maps a distance between the fighter and the camera over some range to a [0,1] value
     */
    private float GetDistanceFactor(float closeDistance, float farDistance)
    {
        return Mathf.Clamp01((Vector3.Distance(Camera.main.transform.position, transform.position) - closeDistance) / (farDistance - closeDistance));
    }
}