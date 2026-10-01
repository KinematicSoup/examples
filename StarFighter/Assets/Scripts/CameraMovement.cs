using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using GamepadInput;
using KS.Reactor.Client.Unity;
using KS.Reactor;

public class CameraMovement : MonoBehaviour
{
    [Tooltip("How much smoothing is applied to the fighter camera.")]
    [Range(0, 2)]
    public float FighterSmoothing = 1;

    [Tooltip("Where the camera resets to while waiting for respawn.")]
    public Transform CameraReset;

    [Tooltip("The camera resets when the player's respawn timer is lower or equal to this.")]
    [Range(0, 5)]
    public float ResetTime = 2f;

    [Tooltip("The camera's normal field of view.")]
    [Range(40, 80)]
    public float DefaultFieldOfView = 60.0f;

    [Tooltip("The max amount the view of view shrinks to while aiming.")]
    [Range(10, 70)]
    public float ZoomFieldOfView = 20.0f;

    [Tooltip("Multiply negative forward and strafe speed properties by this to get the target camera offset.")]
    public float OffsetMultiplier = .15f;

    [Tooltip("Speed to move the camera offset towards the target camera offset.")]
    public float OffsetSpeed = 4f;

    private ChatUI m_chat;
    private ScoreboardUI m_scoreBoard;
    private Transform m_player;
    private Transform m_posTarget;
    private Transform m_posReverseTarget;
    private Transform m_rotTarget;
    private bool m_lookBack = false;
    private float m_fovTarget;

    private ksEntity m_playerEntity;
    private int m_quantizedSpeed;
    private int m_quantizedStrafe;
    private Vector3 m_offset;
    private Vector3 m_targetOffset;

    void Start()
    {
        GameObject ui = GameObject.FindGameObjectWithTag("GUI");
        m_chat = ui.GetComponent<ChatUI>();
        m_scoreBoard = ui.GetComponent<ScoreboardUI>();

        Camera.main.transform.position = CameraReset.position;
        Camera.main.transform.rotation = CameraReset.rotation;

        Camera.main.fieldOfView = DefaultFieldOfView;
        m_fovTarget = DefaultFieldOfView;
    }

    /*
     * makes the camera follow the player.
     */
    void LateUpdate()
    {
        // try find the player fighter if we don't already have it
        if (m_player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            // if we found a player initialize the camera variables
            if (playerObject)
            {
                m_player = playerObject.transform;

                switch (PlayerClient.LocalPlayerType)
                {
                    case PlayerType.FIGHTER:
                        {
                            m_posTarget = m_player.Find("cameraPositionTarget");
                            m_posReverseTarget = m_player.Find("cameraPositionTargetReverse");
                            m_rotTarget = m_player.Find("cameraLookTarget");
                            break;
                        }
                    case PlayerType.TURRET:
                        {
                            m_posTarget = m_player.Find("turret/barrels/cameraPositionTarget");
                            m_posReverseTarget = m_player.Find("turret/barrels/cameraPositionTargetReverse");
                            m_rotTarget = m_player.Find("turret/barrels/cameraLookTarget");

                            transform.SetParent(m_posTarget);
                            break;
                        }
                }
                ksEntityComponent entityComponent = m_posTarget.GetComponentInParent<ksEntityComponent>();
                m_playerEntity = entityComponent == null ? null : entityComponent.Entity;

                transform.position = m_posTarget.position;
                transform.LookAt(m_rotTarget, m_posTarget.up);

                m_offset = Vector3.zero;
                m_quantizedSpeed = 0;
                m_quantizedStrafe = 0;

                Camera.main.fieldOfView = DefaultFieldOfView;
                m_fovTarget = DefaultFieldOfView;
            }
            else if (IsTimeToResetCamera())
            {
                Camera.main.fieldOfView = DefaultFieldOfView;
                m_fovTarget = DefaultFieldOfView;
                Camera.main.transform.position = CameraReset.position;
                Camera.main.transform.rotation = CameraReset.rotation;
            }
        }
        else
        {
            // if the user presses or releases the look backwards button, set the camera's position to the correct side of the fighter
            if (!m_chat.ChatActive && !m_scoreBoard.ShowScores && m_lookBack != Controls.ButtonValue(GameButton.LOOK_BACK))
            {
                if (m_lookBack)
                {
                    transform.position = m_posTarget.position;
                }
                else
                {
                    transform.position = m_posReverseTarget.position;
                } 
                
                transform.LookAt(m_rotTarget, m_posTarget.up);
            }

            if (!m_chat.ChatActive && !m_scoreBoard.ShowScores)
            {
                m_lookBack = Controls.ButtonValue(GameButton.LOOK_BACK);
            }

            if (PlayerClient.LocalPlayerType == PlayerType.FIGHTER)
            {
                Vector3 posTarget = m_lookBack ? m_posReverseTarget.position : m_posTarget.position;

                // SmoothDamp causes jitter, so we don't use it and instead add an offset based on the player's
                // quantized speed and strafe properties.
                if (m_playerEntity != null)
                {
                    int qSpeed = m_playerEntity.Properties[ID.PROP.FIGHTER.FORWARD_SPEED];
                    int qStrafe = m_playerEntity.Properties[ID.PROP.FIGHTER.STRAFE_SPEED];
                    // Don't change offset unless quantized value changes by more than 1 or changes to zero, to
                    // prevent jitter when it alternates between two adjacent values.
                    if (Mathf.Abs(qSpeed - m_quantizedSpeed) > 1 || (qSpeed == 0 && m_quantizedSpeed != 0))
                    {
                        m_quantizedSpeed = qSpeed;
                        m_targetOffset.y = -qSpeed * CONSTS.SPEED_PRECISION * OffsetMultiplier;
                    }
                    if (Mathf.Abs(qStrafe - m_quantizedStrafe) > 1 || (qStrafe == 0 && m_quantizedStrafe != 0))
                    {
                        m_quantizedStrafe = qStrafe;
                        m_targetOffset.x = -qStrafe * CONSTS.SPEED_PRECISION * OffsetMultiplier;
                    }
                    m_offset = Vector2.MoveTowards(m_offset, m_targetOffset, OffsetSpeed * Time.deltaTime);
                    transform.position = posTarget + transform.rotation * new Vector3(m_offset.x, 0f, m_offset.y);
                }
                else
                {
                    transform.position = posTarget;
                }
                transform.LookAt(m_rotTarget, m_posTarget.up);
            }
            else
            {
                TurretClient turret = m_player.GetComponent<TurretClient>();

                // zooms in the camera based on the turret's rotation to avoid clipping with the fighter
                // elevation factor is how much the camera should zoom in based on looking up to prevent clipping with the wings
                // rotation factor is how much to zoom in based on looking behind to prevent clipping with the canopy
                float rotationFactor = -Mathf.Min(Mathf.Abs(Mathf.Abs(turret.Rotation % 360) - 180) - 40, 0) / 40;
                float elevationFactor = Mathf.Clamp01(-Mathf.Min(turret.Elevation + 25, 0) / 35);
                float cameraZoom = Mathf.Clamp01(Ease(elevationFactor) + Ease(rotationFactor * Mathf.Max(elevationFactor - 0.1f, 0) * 4));

                if (!m_lookBack)
                {
                    transform.position = m_posTarget.position + Vector3.Lerp(transform.position - m_posTarget.position, transform.forward * 0.2f * cameraZoom, Time.deltaTime * 10);
                }
                else
                {
                    transform.position = m_posReverseTarget.position;
                }

                if (!m_chat.ChatActive && !m_scoreBoard.ShowScores && Controls.ButtonDown(GameButton.TURRET_AIM))
                {
                    m_fovTarget = m_fovTarget == DefaultFieldOfView ? ZoomFieldOfView : DefaultFieldOfView;
                }

                m_fovTarget = Mathf.Clamp(m_fovTarget - Controls.AxisValue(GameAxis.TURRET_ZOOM), ZoomFieldOfView, DefaultFieldOfView);

                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, m_fovTarget, Time.deltaTime * 8.0f);
            }
        }
    }

    /*
     * how much to reduce turn control sensitivity based on the reduced field of view from zooming to allow for accuracy
     */
    public float DesensitizeFactor()
    {
        return Mathf.Pow(Camera.main.fieldOfView / DefaultFieldOfView, 2);
    }

    /*
     * takes in a value from [0,1] and maps that to a cos function with a range [0,1] to ease the function
     */
    private float Ease(float val)
    {
        return (0.5f * -Mathf.Cos(Mathf.Clamp01(val) * Mathf.PI)) + 0.5f;
    }

    private bool IsTimeToResetCamera()
    {
        if (ksReactor.Service.Rooms.Count == 0)
        {
            return false;
        }
        ksRoom room = (ksRoom)ksReactor.Service.Rooms[0];
        return room.LocalPlayer != null && room.LocalPlayer.Properties[ID.PROP.PLAYER.RESPAWN_TIME_LEFT] <= ResetTime;
    }
}
