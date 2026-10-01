using System;
using System.Collections.Generic;
using KS.Reactor;
using KS.Reactor.Server;

public class Turret : ksServerEntityScript
{
    [ksEditable]
    private float m_maxEnergy = 700f;

    // how much energy is recharged per second
    [ksEditable]
    private float m_energyRechargeRate = 45.0f;
    // how much energy must the fighter accumulate to allow the use of more energy after draining it all
    [ksEditable]
    private float m_energyFreezeEnd = 250.0f;

    // maximum angle the turret guns can be pointed up
    [ksEditable]
    private float m_maxElevation = 75.0f;
    // maximum angle the turret guns can be pointed down
    [ksEditable]
    private float m_maxDepression = 22.5f;
    // how fast the guns can elevate in deg/sec
    [ksEditable]
    private float m_elevateRate = 70.0f;
    // how fast the turret rotates in deg/sec
    [ksEditable]
    private float m_rotateRate = 120.0f;

    // minimum time between laser shots (sec)
    [ksEditable]
    private float m_laserReloadTime = 0.125f;
    // relative to fighter pivot
    [ksEditable]
    private ksVector3 m_turretCenter = new ksVector3(0, 0.081f, -0.101f);
    // relative to turret center
    [ksEditable]
    private ksVector3 m_laserSpawnPosition = new ksVector3(0.0075f, 0, 0.25f);

    private int PlayerId
    {
        get { return Properties[ID.PROP.TURRET.OWNER]; }
        set { Properties[ID.PROP.TURRET.OWNER] = value; }
    }

    private float Energy
    {
        get { return Properties[ID.PROP.TURRET.ENERGY]; }
        set { Properties[ID.PROP.TURRET.ENERGY] = value; }
    }

    private bool EnergyFrozen
    {
        get { return Properties[ID.PROP.TURRET.ENERGY_FREEZE]; }
        set { Properties[ID.PROP.TURRET.ENERGY_FREEZE] = value; }
    }

    private float LaserProgress
    {
        get { return Properties[ID.PROP.TURRET.LASER_RELOAD]; }
        set { Properties[ID.PROP.TURRET.LASER_RELOAD] = value; }
    }

    private ksVector3 Direction
    {
        get { return Properties[ID.PROP.TURRET.DIRECTION]; }
        set { Properties[ID.PROP.TURRET.DIRECTION] = value; }
    }

    private TurretPlayer m_turretPlayer;
    private Timer m_laserReload;

    public bool IsOccupied
    {
        get { return PlayerId != 0; }
    }

    /**
     * Called when the script is attached.
     */
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;

        m_laserReload = new Timer(Time, m_laserReloadTime);
        Energy = m_maxEnergy;
        Properties[ID.PROP.TURRET.MAX_ENERGY] = m_maxEnergy;
    }

    /**
     * Called when the script is detached.
     */
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }

    /*
     * Sets the turret up when a player takes control of it
     */
    public void InitializeTurret(TurretPlayer turretPlayer)
    {
        m_turretPlayer = turretPlayer;
        PlayerId = (int)turretPlayer.Player.Id;
        Direction = Transform.Forward();
    }

    /*
     * When a turret player quits, reset the turret to an unoccupied state.
     */
    public void ResetTurret()
    {
        m_turretPlayer = null;
        PlayerId = 0;
    }

    /**
     * Must be called when the turret is destroyed.
     */
    public void TurretDestroyed(bool losePoints)
    {
        if (m_turretPlayer != null)
        {
            m_turretPlayer.TurretDestroyed(losePoints);
        }
    }

    /*
     * Returns the player currently controlling the turret.
     */
    public Player GetPlayer()
    {
        if (IsOccupied)
        {
            return m_turretPlayer.GetPlayer();
        }
        return null;
    }

    /**
     * Called after the room simulation step.
     */
    public void Update()
    {
        if (GameManager.GameOver || Entity.IsDestroyed)
        {
            return;
        }

        m_laserReload.Update();
        LaserProgress = ksMath.Clamp01(m_laserReload.Time / m_laserReloadTime + (m_laserReload.IsDone && !CanUseEnergy() ? 1 : 0));

        Energy = Math.Min(Energy + m_energyRechargeRate * Time.Delta, m_maxEnergy);
        // makes the energy avaliable to use again after enough is accumulated if it was all used up
        EnergyFrozen = EnergyFrozen && Energy < m_energyFreezeEnd;

        if (IsOccupied)
        {
            ksVector3 localDirection = Direction * Transform.Rotation.Inverse();
            ksVector3 euler = ksQuaternion.FromDirection(localDirection, Transform.Up()).ToEuler();
            if (euler.X < -m_maxElevation || euler.X > m_maxDepression)
            {
                euler.X = ksMath.Clamp(euler.X, -m_maxElevation, m_maxDepression);
                localDirection = ksVector3.Forward * ksQuaternion.FromEuler(euler);
                Direction = localDirection * Transform.Rotation;
            }
        }
    }

    /*
     * Checks if the fighter is able to spend energy.
     */
    private bool CanUseEnergy()
    {
        return Energy >= 0 && !EnergyFrozen;
    }

    /*
     * Lowers the fighte's energy pool if possible and checks if the fighter ran out of energy, temporarily 
     * freezing its use. Returns wether or not the energy cost could be afforded.
     */
    private bool SpendEnergy(float energyCost)
    {
        if (Energy >= 0 && !EnergyFrozen)
        {
            Energy -= energyCost;
            EnergyFrozen = Energy < 0;
            return true;
        }
        return false;
    }

    /**
     * Fires a laser when instructed by the client when possible.
     */
    [ksRPC(ID.RPC.TURRET_LASER)]
    private void FireLaser(ksIServerPlayer player)
    {
        if (player == m_turretPlayer.Player && !GameManager.GameOver && m_laserReload.IsDone && CanUseEnergy())
        {
            m_laserReload.Start();

            ksQuaternion spawnRotation = ksQuaternion.FromDirection(Direction, Transform.Up());
            ksVector3 spawnPosition = Transform.ToWorld(m_turretCenter) + m_laserSpawnPosition * spawnRotation;
            m_laserSpawnPosition.X = -m_laserSpawnPosition.X;

            Laser laser = Room.SpawnEntity(ID.TYPE.LASER, spawnPosition, spawnRotation, ksVector3.One)
                .Scripts.Get<Laser>();
            laser.InitializeLaser(GetPlayer(), Properties[ID.PROP.FIGHTER.LASER_SPEED]);
            SpendEnergy(laser.EnergyCost);
        }
    }

    /**
     * Rotates the barrels up and down within the extent permitted.
     */
    [ksRPC(ID.RPC.TURRET_ELEVATION)]
    private void ElevateGuns(ksIServerPlayer player, float elevation)
    {
        if (player == m_turretPlayer.Player && !GameManager.GameOver)
        {
            ksVector3 oldDirection = Direction;
            ksQuaternion rotation = ksQuaternion.FromDirection(Direction, Transform.Up());
            Direction *= ksQuaternion.FromAxisAngle(ksVector3.Right * rotation, -elevation);
        }
    }

    /**
     * Rotates the turret around the up axis.
     */
    [ksRPC(ID.RPC.TURRET_ROTATION)]
    private void RotateTurret(ksIServerPlayer player, float rotation)
    {
        if (player == m_turretPlayer.Player && !GameManager.GameOver)
        {
            Direction *= ksQuaternion.FromAxisAngle(Transform.Up(), rotation);
        }
    }
}