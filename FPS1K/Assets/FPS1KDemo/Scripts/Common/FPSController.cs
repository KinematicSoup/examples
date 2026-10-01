using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor;

// First person controller using kinematic motion and sweep-and-slide movement.
public class FPSController : ksPlayerController
{
    // Sometimes a surface rotated 45 degrees that should have a slope of 1 has a slope barely above 1 which stops it
    // from being walked on when the MaxSlope is 1. To prevent this, we increase the MaxSlope by this much when testing
    // if a slope can be walked on.
    private const float SLOPE_TOLERANCE = 1E-6F;
    // Horizontal distance for step-up checks. Using the horizontal velocity to determine the location of step checks
    // causes inconsistent behaviour with different velocities because the vertical step distance can vary at different
    // points on a slope, so for more consistent stepping behaviour we used a fixed horizontal distance for step-up
    // checks.
    private const float HORIZONTAL_STEP_DISTANCE = .1f;
    // Max number of sweep-and-slide iterations.
    private const int MAX_SWEEP_ITERATIONS = 4;
    // It's possible to get stuck when you are just touching an object where sweeping in any direction will result in a
    // hit with zero distance because the sweep considers it an initial overlap, but overlap queries do not detect the
    // object so you cannot depenetrate and you cannot move in any direction. If we detect that ResolvePenetrations found
    // no overlaps but the sweep wasn't able to move, we move the character forward this amount to either move us further
    // into the adjacent object so we can depenetrate from it, or move us away from the object.
    private const float STUCK_OFFSET = .001f;

    [ksEditable]
    public float MoveSpeed = 5f;
    [ksEditable]
    public float DashSpeed = 8f;
    [ksEditable]
    public float JumpSpeed = 6f;
    // Max angle in degrees players can look up or down.
    [ksEditable]
    public float MaxPitch = 75f;
    // Max slope the player can walk up.
    [ksEditable]
    public float MaxSlope = 1f;
    // How far the player can move to snap to the ground. The player only snaps to the ground if it was on the ground
    // the previous frame and did not jump.
    [ksEditable]
    public float SnapGroundDistance = .05f;
    // How far the player can step-up if they are on the ground.
    [ksEditable]
    public float StepDistance = .25f;
    // Maximum angle away from the target direction the player can slide.The player will stop moving if the slide
    // angle exceeds this.
    [ksEditable]
    public float MaxSlideAngle = 80f;
    // Do not move if the remaining move distance is less than this.
    [ksEditable]
    public float MinMoveDistance = .01f;
    [ksEditable]
    // Multiplier for impulses applied to dynamic rigid bodies the character collides with.
    public float ImpulseMultiplier = .5f;
    // Maximum distance the character can penetrate other objects. When this is exceeded, the OnCrush delegate is
    // called (which kills the player). This should not be greater than the capsule radius, or the character may get
    // squished between two objects horizontally without dieing.
    [ksEditable]
    public float MaxPenetration = 1f / 4f;

    public IWeapon Weapon;

    // The collider used for collision detection.
    public ksICollider Collider
    {
        get { return m_sweepSlideArgs.Collider; }
        set { m_sweepSlideArgs.Collider = value; }
    }

    public delegate void LandDelegate();
    public LandDelegate OnLand;

    public delegate void CrushDelegate();
    public CrushDelegate OnCrush;

    public delegate void CollideDelegate(FPSController controller, ksSweepResult hit);
    public CollideDelegate OnCollide;

    public delegate void PickUpDelegate(ksIEntity entity);
    public PickUpDelegate OnPickUp;

    public delegate ksVector3 GroundVelocityGetter(ksSweepResult hit);
    public GroundVelocityGetter GetGroundVelocity;

    public delegate bool IsDestroyedDelegate();
    public IsDestroyedDelegate IsDestroyed;

    // Does the character's head position need to be updated?
    public bool HeadPositionStale = false;

    public ksVector2 Aim
    {
        get { return m_aim; }
    }

    private bool m_onGround = false;
    private bool m_hitWall = false;

    private ksSweepSlideParams m_sweepSlideArgs = new ksSweepSlideParams();
    private ksVector2 m_aim = new ksVector2(-1000f, -1000f);
    private ksVector2 m_direction;
    private ksVector3 m_sweepDelta;
    private uint m_moveState;
    private bool m_rotating = true;
    private bool m_airDash = false;
    private bool m_stuck = false;
    private int[] m_quantizedAim = new int[2];

    public ksVector3 Velocity
    {
        get { return m_velocity; }
        set { m_velocity = value; }
    }
    private ksVector3 m_velocity;

    // Cannot move while stunned until you touch the ground.
    public bool Stunned
    {
        get { return Properties[Prop.STUNNED]; }
        set 
        {
            if (Entity.IsDestroyed)
            {
                return;
            }
            if (value)
            {
                m_onGround = false;
            }
            Properties[Prop.STUNNED] = value;
        }
    }

    private ksVector3 m_groundVelocity;

    // Unique non-zero identifier for this player controller class.
    public override uint Type
    {
        get { return 1; }
    }

    // Register all buttons and axes you will be using here.
    public override void RegisterInputs(ksInputRegistrar registrar)
    {
        registrar.RegisterButtons(Buttons.SHOOT, Buttons.JUMP, Buttons.DASH, Buttons.VERIFY_SHOT);
        registrar.RegisterAxes(Axes.X, Axes.Z, Axes.YAW, Axes.PITCH);
    }

    public override void Initialize()
    {
        m_sweepSlideArgs.Filter = Entity.CollisionFilter;
        m_sweepSlideArgs.ExcludeEntity = Entity;
        m_sweepSlideArgs.MaxSlideAngle = MaxSlideAngle;
        m_sweepSlideArgs.Flags |= ksQueryFlags.EXCLUDE_TOUCHES;
        m_sweepSlideArgs.Flags &= ~ksQueryFlags.EXCLUDE_OVERLAPS;
        m_sweepSlideArgs.MaxIterations = MAX_SWEEP_ITERATIONS;
        m_sweepSlideArgs.CollideCallback = HandleCollision;
    }

    // Called every frame.
    public override void Update()
    {
        // Set aim to yaw and pitch to values sent from client.
        ksVector2 aim = new ksVector2(Input.GetAxis(Axes.PITCH) * MaxPitch, Input.GetAxis(Axes.YAW) * 180f);
        if (aim != m_aim)
        {
            QuantizeAim(aim);
            Properties[Prop.AIM] = m_quantizedAim;
            m_direction = ksVector2.FromDegrees(90f - aim.Y);
            m_aim = aim;
            m_rotating = true;
            HeadPositionStale = true;
        }

        if (m_onGround)
        {
            Stunned = false;
        }
        if (!Stunned)
        {
            m_velocity.X = m_groundVelocity.X;
            m_velocity.Z = m_groundVelocity.Z;
        }

        // Apply walk inputs
        ksVector2 input = new ksVector2(Input.GetAxis(Axes.X), Input.GetAxis(Axes.Z));
        ksVector2 walkVelocity;
        uint moveState;
        if ((input.X == 0f && input.Y == 0f) || Stunned)
        {
            moveState = MoveStates.IDLE;
            walkVelocity = ksVector2.Zero;
            m_airDash = false;
        }
        else
        {
            if (!m_onGround)
            {
                m_airDash = (m_airDash | m_moveState == MoveStates.SPRINT) && Input.IsDown(Buttons.DASH) &&
                    input.Y > 0f;
                moveState = MoveStates.IDLE;
            }
            else
            {
                m_airDash = false;
                moveState = input.Y < 0f ? MoveStates.BACKWARDS : MoveStates.FORWARD;
                if (Input.IsDown(Buttons.DASH) && !Input.IsDown(Buttons.SHOOT) && input.Y > 0f)
                {
                    moveState = MoveStates.SPRINT;
                }
            }

            input.Clamp(1f);
            float speed = moveState == MoveStates.SPRINT || m_airDash ? DashSpeed : MoveSpeed;
            walkVelocity = (m_direction * input.Y + new ksVector2(m_direction.Y, -m_direction.X) * input.X) * speed;
            m_velocity += new ksVector3(walkVelocity.X, 0f, walkVelocity.Y);
        }
        if (moveState != m_moveState)
        {
            Properties[Prop.MOVE_STATE] = moveState;
            m_moveState = moveState;
            m_rotating = true;
        }

        if (Input.IsPressed(Buttons.JUMP) && m_onGround)
        {
            m_velocity.Y = JumpSpeed + m_groundVelocity.Y;
            // Set on ground to false when jumping to avoid snapping to the ground.
            m_onGround = false;
        }

        // Update rotation
        if (m_rotating)
        {
            UpdateRotation(walkVelocity);
        }

        // Update weapon
        if (Weapon != null && !Stunned)
        {
            Weapon.OnUpdate(Input);
        }

        // Apply velocity
        Move();
    }

    private void UpdateRotation(ksVector2 walkVelocity)
    {
        ksQuaternion rotation = Transform.Rotation;
        switch (m_moveState)
        {
            default:
            case MoveStates.IDLE:
            {
                ksVector2 forward = Transform.Forward().XZ;
                if (Math.Abs(ksVector2.DeltaDegrees(forward, m_direction)) > 45f)
                {
                    Transform.RotateTowards(Transform.Position + new ksVector3(m_direction.X, 0f, m_direction.Y),
                        360f * Time.Delta);
                }
                break;
            }
            case MoveStates.SPRINT:
            case MoveStates.FORWARD:
            {
                Transform.RotateTowards(Transform.Position + new ksVector3(walkVelocity.X, 0f, walkVelocity.Y),
                    360f * Time.Delta);
                break;
            }
            case MoveStates.BACKWARDS:
            {
                Transform.RotateTowards(Transform.Position - new ksVector3(walkVelocity.X, 0f, walkVelocity.Y),
                    360f * Time.Delta);
                break;
            }
        }
        m_rotating = Transform.Rotation != rotation;
        HeadPositionStale |= m_rotating;
    }

    private void Move()
    {
        if (Time.Delta <= 0)
        {
            return;
        }
        m_velocity += Physics.Gravity * Time.Delta;
        ksVector3 moveDelta = m_velocity / 60f;
        if (moveDelta.MagnitudeSquared() < MinMoveDistance * MinMoveDistance)
        {
            return;
        }
        bool wasOnGround = m_onGround;
        m_onGround = false;
        m_hitWall = false;
        m_groundVelocity = ksVector3.Zero;
        ksVector3 start = Transform.Position;
        if (RigidBody != null)
        {
            start += RigidBody.KinematicMovement;
        }
        bool penetrated = ResolvePenetrations(ref start);
        ksVector3 end;
        m_sweepSlideArgs.Origin = start;
        m_sweepDelta = m_velocity * Time.Delta;
        m_sweepSlideArgs.End = start + m_sweepDelta;
        float ySpeed = m_velocity.Y;
        m_stuck = false;
        bool collided = Physics.SweepAndSlide(m_sweepSlideArgs, out end);
        if (collided)
        {
            ksVector2 horizontalVelocity = m_velocity.XZ;
            // Calculate new velocity.
            m_velocity = (end - start) / Time.Delta;
            // If we were on the ground, hit a wall, and our horizontal velocity was non-zero, check if we can step up.
            if (wasOnGround && m_hitWall && horizontalVelocity != ksVector2.Zero && StepDistance > 0f)
            {
                ksVector3 stepPosition = end +  new ksVector3(horizontalVelocity.X, 0f, horizontalVelocity.Y)
                    .Normalized() * HORIZONTAL_STEP_DISTANCE;
                if (DetectStep(ref stepPosition))
                {
                    end = m_sweepSlideArgs.End;
                    end.Y = stepPosition.Y;
                }
            }
            if (m_stuck && start == end)
            {
                end += m_sweepSlideArgs.Direction * STUCK_OFFSET;
            }
        }
        if (!m_onGround)
        {
            if (wasOnGround)
            {
                m_onGround = SnapToGround(ref end);
            }
            if (!m_onGround)
            {
                // Keep accumulating gravity while not on the ground to force the player to slide down steep slopes.
                m_velocity.Y = ySpeed;
            }
        }
        else if (!wasOnGround && OnLand != null)
        {
            OnLand();
        }
        if (RigidBody != null)
        {
            RigidBody.KinematicMovement = end - Transform.Position;
        }
        else
        {
            Transform.Position = end;
        }
    }

    private float CalculateSlope(ksVector3 normal)
    {
        if (normal.Y == 0)
        {
            return float.MaxValue;
        }
        float horizontal = normal.XZ.Magnitude();
        return horizontal / normal.Y;
    }

    private bool HandleCollision(
        ksSweepResult hit,
        ksSweepSlideParams args,
        ksVector3 direction,
        ksSweepSlideParams.CallbackStates state)
    {
        if (state == ksSweepSlideParams.CallbackStates.ANGLE_LIMIT_REACHED && hit.Distance == 0f)
        {
            m_stuck = true;
        }
        if (OnCollide != null)
        {
            OnCollide(this, hit);
        }
        float slope = CalculateSlope(hit.Normal);
        if (hit.Normal.Y > 0 && slope <= MaxSlope + SLOPE_TOLERANCE)
        {
            if (!m_onGround && GetGroundVelocity != null)
            {
                m_groundVelocity = GetGroundVelocity(hit);
            }
            m_onGround = true;
            // Don't slide down slopes if you are on ground.
            if (state == ksSweepSlideParams.CallbackStates.NEW_SWEEP && args.Direction.Y < 0f)
            {
                if ((direction.X == 0f && direction.Z == 0f) || (args.Direction.X == 0f && args.Direction.Z == 0f))
                {
                    return false;
                }
                float originalDH = m_sweepDelta.Magnitude();
                if (originalDH == 0f)
                {
                    return false;
                }
                float dh = (args.Direction * args.Distance).XZ.Magnitude();
                if (dh == 0)
                {
                    return false;
                }
                args.Distance *= originalDH / dh;
            }
        }
        else if (Math.Abs(slope) > MaxSlope + SLOPE_TOLERANCE)
        {
            m_hitWall = true;
        }
        if (args.Distance < MinMoveDistance)
        {
            return false;
        }
        m_sweepDelta = args.Direction * args.Distance;
        return true;
    }

    private bool SnapToGround(ref ksVector3 position)
    {
        ksSweepResult hit;
        ksVector3 groundPosition;
        m_sweepSlideArgs.Origin = position;
        m_sweepSlideArgs.Direction = ksVector3.Down;
        m_sweepSlideArgs.Distance = SnapGroundDistance;
        if (SweepNearest(out hit, out groundPosition) && hit.Normal.Y > 0 && 
            CalculateSlope(hit.Normal) <= MaxSlope + SLOPE_TOLERANCE)
        {
            position = groundPosition + ksVector3.Up * m_sweepSlideArgs.SeparationOffset;
            if (GetGroundVelocity != null)
            {
                m_groundVelocity = GetGroundVelocity(hit);
            }
            return true;
        }
        return false;
    }

    private bool DetectStep(ref ksVector3 position)
    {
        ksSweepResult hit;
        ksVector3 stepPosition;
        m_sweepSlideArgs.Origin = position + ksVector3.Up * StepDistance;
        m_sweepSlideArgs.Direction = ksVector3.Down;
        m_sweepSlideArgs.Distance = StepDistance;
        if (SweepNearest(out hit, out stepPosition))
        {
            position = stepPosition + ksVector3.Up * m_sweepSlideArgs.SeparationOffset;
            return true;
        }
        return false;
    }

    // Attempts to push the entity out of overlapping geometry.
    private bool ResolvePenetrations(ref ksVector3 position)
    {
        bool penetrated = false;
        m_sweepSlideArgs.Origin = position;
        m_sweepSlideArgs.Flags &= ~ksQueryFlags.EXCLUDE_TOUCHES;
        List<ksOverlapResult> overlaps = Physics.Overlap(m_sweepSlideArgs);
        m_sweepSlideArgs.Flags |= ksQueryFlags.EXCLUDE_TOUCHES;
        foreach (ksOverlapResult overlap in overlaps)
        {
            if (overlap.Collider.CollisionFilter.Group == Collision.PICKUP)
            {
                if (OnPickUp != null)
                {
                    OnPickUp(overlap.Entity);
                }
                continue;
            }

            ksVector3 direction;
            float distance;
            // Compute the penetration for this overlap with the delta from previous overlaps applied.
            if (Physics.ComputePenetration(overlap.QueryCollider, position, Transform.Rotation, overlap.Collider,
                overlap.Entity.Transform.Position, overlap.Entity.Transform.Rotation, out direction, out distance))
            {
                if (distance > MaxPenetration && OnCrush != null)
                {
                    OnCrush();
                    return true;
                }
                penetrated = true;
                position += direction * (distance + m_sweepSlideArgs.SeparationOffset);
            }
        }
        return penetrated;
    }

    private bool SweepNearest(out ksSweepResult hit, out ksVector3 position)
    {
        ksSweepResult hitResult = new ksSweepResult();
        m_sweepSlideArgs.MaxIterations = 1;
        m_sweepSlideArgs.CollideCallback = (
            ksSweepResult nearestHit,
            ksSweepSlideParams args,
            ksVector3 direction,
            ksSweepSlideParams.CallbackStates state) =>
        {
            hitResult = nearestHit;
            return false;
        };
        // We use Physics.SweepAndSlide with 1 iteration (no sliding) instead of Physics.SweepNearest to make use of
        // ksSweepSlideParams.SeparationOffset
        m_sweepSlideArgs.Flags |= ksQueryFlags.EXCLUDE_OVERLAPS;
        bool collided = Physics.SweepAndSlide(m_sweepSlideArgs, out position);
        m_sweepSlideArgs.Flags &= ~ksQueryFlags.EXCLUDE_OVERLAPS;
        m_sweepSlideArgs.MaxIterations = MAX_SWEEP_ITERATIONS;
        m_sweepSlideArgs.CollideCallback = HandleCollision;
        hit = hitResult;
        return collided;
    }

    private void QuantizeAim(ksVector2 aim)
    {
        m_quantizedAim[0] = (int)Math.Round(aim.X);
        m_quantizedAim[1] = (int)Math.Round(aim.Y);
    }
}