using System;
using KS.Reactor;

/*
 * Controls the movement of the player fighter.
 */
public class FighterController : ksPlayerController
{
    public static bool InputPredictionEnabled;

    [ksEditable]
    private float m_speedSmoothing = 0.05f;
    [ksEditable]
    private float m_rollSmoothing = 0.2f;
    [ksEditable]
    private float m_strafeSmoothing = 0.05f;
    [ksEditable]
    private float m_deaccelerationSmoothing = 0.01f;
    [ksEditable]
    private float m_maxForward = 6.0f;
    [ksEditable]
    private float m_maxReverse = 3.0f;
    [ksEditable]
    private float m_maxStrafe = 4.0f;
    [ksEditable]
    private float m_maxTurnSpeed = 90.0f;
    [ksEditable]
    private float m_maxRollSpeed = 150.0f;

    private bool m_laser;
    public bool Laser
    {
        get { return m_laser; }
    }

    private bool m_missile;
    public bool Missile
    {
        get { return m_missile; }
    }

    private bool m_useShield;
    public bool UseShield
    {
        get { return m_useShield; }
    }

    private bool m_useBoost;
    public bool UseBoost
    {
        get { return m_useBoost; }
    }

    private float m_speed = 0;
    private float m_strafe = 0;
    private float m_roll = 0;

    public override uint Type
    {
        get { return ID.CONTROLLERS.FIGHTER; } // must be a unique non-zero identifier
    }

    public override void RegisterInputs(ksInputRegistrar registrar)
    {
        registrar.RegisterAxes(
            ID.CONTROLS.TURN_X,
            ID.CONTROLS.TURN_Y,
            ID.CONTROLS.ROLL,
            ID.CONTROLS.ACCELERATE,
            ID.CONTROLS.STRAFE
        );
        registrar.RegisterButtons(
            ID.CONTROLS.LASER,
            ID.CONTROLS.MISSILE,
            ID.CONTROLS.SHIELD,
            ID.CONTROLS.BOOST
        );
    }

    /*
     * sets the speed at spawn to the average speed of the fighter
     */
    public override void Initialize()
    {
        m_speed = m_maxForward / 2;
        UseInputPrediction = InputPredictionEnabled;
    }

    public override void Update()
    {
        m_laser = Input.IsDown(ID.CONTROLS.LASER);
        m_missile = Input.IsDown(ID.CONTROLS.MISSILE);
        m_useShield = Input.IsDown(ID.CONTROLS.SHIELD);
        m_useBoost = Input.IsDown(ID.CONTROLS.BOOST);

        float pitchInput = -Input.GetAxis(ID.CONTROLS.TURN_Y);
        float yawInput = Input.GetAxis(ID.CONTROLS.TURN_X);
        float rollInput = -Input.GetAxis(ID.CONTROLS.ROLL);
        float strafeInput = Input.GetAxis(ID.CONTROLS.STRAFE);
        float accelerateInput = Input.GetAxis(ID.CONTROLS.ACCELERATE);

        ksVector2 turningInput = new ksVector2(yawInput, pitchInput).Clamped(1.0f);

        float magnitude = (float)Math.Sqrt((double)(strafeInput * strafeInput + accelerateInput * accelerateInput));
        float targetSpeed = magnitude == 0 ? 0 : (accelerateInput / magnitude);
        float targetStrafe = magnitude == 0 ? 0 : (strafeInput / magnitude);
        targetSpeed = targetSpeed * ((accelerateInput > 0) ? m_maxForward : (accelerateInput < 0) ? m_maxReverse : 0);
        targetStrafe = targetStrafe * m_maxStrafe;

        if (m_useBoost && Properties[ID.PROP.FIGHTER.BOOST_RELOAD] == 0)
        {
            targetSpeed *= 2.0f;
            targetStrafe *= 2.0f;
        }

        float t = Time.Delta * 60f;
        if (Math.Abs(accelerateInput) > 0)
        {
            m_speed = ksMath.Lerp(m_speed, targetSpeed, m_speedSmoothing * t);
        }
        else
        {
            m_speed = ksMath.Lerp(m_speed, 0, m_deaccelerationSmoothing * t);
        }

        if (Math.Abs(strafeInput) > 0)
        {
            m_strafe = ksMath.Lerp(m_strafe, targetStrafe, m_strafeSmoothing * t);
        }
        else
        {
            m_strafe = ksMath.Lerp(m_strafe, 0, m_deaccelerationSmoothing * t);
        }

        Properties[ID.PROP.FIGHTER.FORWARD_SPEED] = Utils.Quantize(m_speed, CONSTS.SPEED_PRECISION);
        Properties[ID.PROP.FIGHTER.STRAFE_SPEED] = Utils.Quantize(m_strafe, CONSTS.SPEED_PRECISION);

        float pitch = turningInput.Y * m_maxTurnSpeed;
        float yaw = turningInput.X * m_maxTurnSpeed;
        m_roll = ksMath.Lerp(m_roll, rollInput * m_maxRollSpeed, m_rollSmoothing * t);

        ksVector3 velocity = Transform.Forward() * m_speed + Transform.Right() * m_strafe;
        RigidBody.Velocity = velocity;

        ksVector3 angularVelocity = Transform.Rotation * new ksVector3(pitch, yaw, m_roll);
        RigidBody.AngularVelocity = angularVelocity;
    }
}