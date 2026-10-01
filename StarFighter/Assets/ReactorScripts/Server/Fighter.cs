using KS.Reactor;
using KS.Reactor.Server;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Controls the non-input aspects of a player fighter.
 */
public class Fighter : ksServerEntityScript
{
    public static List<Fighter> Instances
    {
        get { return m_instances; }
    }
    private static List<Fighter> m_instances = new List<Fighter>();

    [ksEditable]
    private int m_maxHealth = 1200;
    [ksEditable]
    private float m_maxEnergy = 1000f;

    [ksEditable]
    private int m_missileCapacity = 3;
    [ksEditable]
    private int m_startingMissiles = 1;

    [ksEditable]
    private int m_shieldCapacity = 3;
    [ksEditable]
    private int m_startingShields = 1;

    // the score gained by a player upon destoying this fighter
    [ksEditable]
    private int m_scoreValue = 18;

    // how much energy is replaced per second
    [ksEditable]
    private float m_energyRechargeRate = 50.0f;
    // how much energy must the fighter accumulate to allow the use of more energy after draining it all
    [ksEditable]
    private float m_energyFreezeEnd = 250.0f;
    // the percent of damage that is taken while the shield is active
    [ksEditable]
    private float m_shieldDamageFraction = 0.10f;
    // how much energy is used when boosting per second
    [ksEditable]
    private float m_boostEnergyDrain = 150.0f;

    // radius past which players are declaired out of the map
    [ksEditable]
    private float m_outOfBoundsRadius = 150.0f;
    // how long a player can remain out of bounds before being destroyed (sec) 
    [ksEditable]
    private float m_outOfBoundsSafeTime = 10.0f;

    [ksEditable]
    private float m_laserSpeed = 45f;
    // minimum time between laser shots (sec)
    [ksEditable]
    private float m_laserReloadTime = 0.165f;
    [ksEditable]
    private ksVector3 m_laserSpawnPosition = new ksVector3(-0.1095f, -0.0425f, 0.0f);

    // How long it takes for the missile to lock onto the target before firing (sec)
    [ksEditable]
    private float m_missileLockTime = 3.0f;
    // minimum time between missile shots (sec)
    [ksEditable]
    private float m_missileReloadTime = 15.0f;
    // The forward vector of this fighter and a target fighter must have a dot product greater than this to lock the missile
    [ksEditable]
    private float m_missileMinDot = 0.95f;
    [ksEditable]
    private ksVector3 m_missileSpawnPosition = new ksVector3(0, -0.041f, 0.5f);

    // Minimum impulse needed from collisions to cause damage to the fighter.
    [ksEditable]
    private float m_minImpulseForDamage = 1f;
    // Damage to take from collisions. This is multiplied by the impulse minus the minimum impulse for damage.
    [ksEditable]
    private float m_impulseDamage = 40f;

    public int Health
    {
        get { return Properties[ID.PROP.FIGHTER.HEALTH]; }
        set { Properties[ID.PROP.FIGHTER.HEALTH] = value; }
    }

    private float Energy
    {
        get { return Properties[ID.PROP.FIGHTER.ENERGY]; }
        set { Properties[ID.PROP.FIGHTER.ENERGY] = value; }
    }

    private bool EnergyFrozen
    {
        get { return Properties[ID.PROP.FIGHTER.ENERGY_FREEZE]; }
        set { Properties[ID.PROP.FIGHTER.ENERGY_FREEZE] = value; }
    }

    private bool UsingShield
    {
        get { return Properties[ID.PROP.FIGHTER.USING_SHIELD]; }
        set { Properties[ID.PROP.FIGHTER.USING_SHIELD] = value; }
    }

    private float LaserReload
    {
        get { return Properties[ID.PROP.FIGHTER.LASER_RELOAD]; }
        set { Properties[ID.PROP.FIGHTER.LASER_RELOAD] = value; }
    }

    private float MissileReload
    {
        get { return Properties[ID.PROP.FIGHTER.MISSILE_RELOAD]; }
        set { Properties[ID.PROP.FIGHTER.MISSILE_RELOAD] = value; }
    }

    private float BoostReload
    {
        get { return Properties[ID.PROP.FIGHTER.BOOST_RELOAD]; }
        set { Properties[ID.PROP.FIGHTER.BOOST_RELOAD] = value; }
    }

    private float ShieldReload
    {
        get { return Properties[ID.PROP.FIGHTER.SHIELD_RELOAD]; }
        set { Properties[ID.PROP.FIGHTER.SHIELD_RELOAD] = value; }
    }

    private uint LockTarget
    {
        get { return Properties[ID.PROP.FIGHTER.LOCK_TARGET]; }
        set { Properties[ID.PROP.FIGHTER.LOCK_TARGET] = value; }
    }

    private bool IsTargeted
    {
        get { return Properties[ID.PROP.FIGHTER.IS_TARGETED]; }
        set { Properties[ID.PROP.FIGHTER.IS_TARGETED] = value; }
    }

    private bool IsOutOfBounds
    {
        get { return Properties[ID.PROP.FIGHTER.OUT_OF_BOUNDS]; }
        set { Properties[ID.PROP.FIGHTER.OUT_OF_BOUNDS] = value; }
    }

    private int MissileCount
    {
        get { return Properties[ID.PROP.FIGHTER.MISSILE_COUNT]; }
        set { Properties[ID.PROP.FIGHTER.MISSILE_COUNT] = value; }
    }

    private int ShieldCount
    {
        get { return Properties[ID.PROP.FIGHTER.SHIELD_COUNT]; }
        set { Properties[ID.PROP.FIGHTER.SHIELD_COUNT] = value; }
    }

    public Team Team
    {
        get { return m_player.Team; }
    }

    private Player m_player;
    private ksRandom m_rand = new ksRandom();
    private Timer m_laserReload;
    private Timer m_missileReload;
    private Timer m_missileLock;
    private Timer m_outOfBounds;
    private float m_laserShootSide = 1;
    private ksIServerEntity m_missileLockTarget = null;
    private ArrayList m_incomingMissiles = new ArrayList();
    private ksVector3 m_crosshairTarget;
    private ksVector3 m_velocity = ksVector3.Zero;
    private float m_shieldTime = 0.0f;
    private ksRigidBody m_rigidBody;
    private Turret m_turret;

    /**
     * Called when the script is attached.
     */
    public override void Initialize()
    {
        m_rigidBody = Scripts.Get<ksRigidBody>();
        m_turret = Scripts.Get<Turret>();
        m_player = Entity.Owner.Scripts.Get<Player>();
        Room.OnUpdate[0] += Update;
        Entity.OnCollision += OnCollision;
        Entity.OnOverlapStart += OnOverlap;
        Entity.OnDestroy += SpawnCrates;

        m_laserReload = new Timer(Time, m_laserReloadTime);
        m_missileReload = new Timer(Time, m_missileReloadTime);
        m_missileLock = new Timer(Time, m_missileLockTime);
        m_outOfBounds = new Timer(Time, m_outOfBoundsSafeTime);

        Health = m_maxHealth;
        Energy = m_maxEnergy;
        Properties[ID.PROP.FIGHTER.MAX_HEALTH] = m_maxHealth;
        Properties[ID.PROP.FIGHTER.MAX_ENERGY] = m_maxEnergy;
        Properties[ID.PROP.FIGHTER.MISSILE_CAPACITY] = m_missileCapacity;
        Properties[ID.PROP.FIGHTER.SHIELD_CAPACITY] = m_shieldCapacity;
        Properties[ID.PROP.FIGHTER.LASER_SPEED] = m_laserSpeed;
        MissileCount = m_startingMissiles;
        ShieldCount = m_startingShields;

        m_instances.Add(this);
    }

    /**
     * Called when the script is detached.
     */
    public override void Detached()
    {
        m_instances.Remove(this);

        Room.OnUpdate[0] -= Update;
        Entity.OnCollision -= OnCollision;
        Entity.OnOverlapStart -= OnOverlap;
        Entity.OnDestroy -= SpawnCrates;
    }

    /*
     * Takes a missile to keep track of so the player may be infromed about incoming missiles.
     */
    public void AddIncomingMissile(ksIServerEntity missile)
    {
        m_incomingMissiles.Add(missile);
    }

    /*
     * Returns the pilot and gunner players if avaliable.
     */
    public Player[] GetPlayers()
    {
        Player player = (Entity.Owner != null) ? Entity.Owner.Scripts.Get<Player>() : null;
        if (player != null)
        {
            if (m_turret.IsOccupied)
            {
                return new Player[] { Entity.Owner.Scripts.Get<Player>(), m_turret.GetPlayer() };
            }
            else
            {
                return new Player[] { Entity.Owner.Scripts.Get<Player>() };
            }
        }
        return null;
    }


    /**
     * Collect pickups or take damage from missiles. Laser hits are handled by the laser script using sweep queries.
     */
    private void OnOverlap(ksOverlap overlap)
    {
        if (Health <= 0 || GameManager.GameOver)
        {
            return;
        }

        switch (overlap.Entity1.Type)
        {
            case ID.TYPE.MISSILE:
                Missile missile = overlap.Entity1.Scripts.Get<Missile>();
                Hit(overlap.Entity1, missile.Damage, missile.Owner);
                break;
            case ID.TYPE.MISSILE_CRATE:
                if (!overlap.Entity1.IsDestroyed && MissileCount < m_missileCapacity)
                {
                    MissileCount++;
                    overlap.Entity1.Destroy();
                }
                break;
            case ID.TYPE.SHIELD_CRATE:
                if (!overlap.Entity1.IsDestroyed && ShieldCount < m_shieldCapacity)
                {
                    ShieldCount++;
                    overlap.Entity1.Destroy();
                }
                break;
            case ID.TYPE.HEALTH_CRATE:
                if (!overlap.Entity1.IsDestroyed && Health < m_maxHealth)
                {
                    Health += m_maxHealth / 2;
                    if (Health > m_maxHealth)
                    {
                        Health = m_maxHealth;
                    }
                    overlap.Entity1.Destroy();
                }
                break;
        }
    }

    /*
     * Takes damage resulting from collisions with the enviroment or other players.
     */
    private void OnCollision(ksContact contact)
    {
        // in case we already registered a collision this frame, ignore any that occur after the fighter's destruction
        if (Health <= 0 || GameManager.GameOver)
        {
            return;
        }

        switch (contact.Entity1.Type)
        {
            case ID.TYPE.FIGHTER:
                Health = 0;
                FighterDestroyed(false);
                // only let one fighter call the collision broadcast
                if (contact.Entity1.Scripts.Get<Fighter>().Health <= 0)
                {
                    Room.Scripts.Get<Chat>().BroadcastFighterCollision(GetPlayers(), contact.Entity1.Scripts.Get<Fighter>().GetPlayers());
                }
                break;
            default:
                if (!UsingShield)
                {
                    float impulse = contact.Impulse.Magnitude();
                    if (impulse > m_minImpulseForDamage)
                    {
                        Entity.CallRPC(ID.RPC.FIGHTER_HIT);
                        Health -= (int)(m_impulseDamage * (impulse - m_minImpulseForDamage));
                    }

                    if (Health <= 0)
                    {
                        FighterDestroyed(false);
                        Room.Scripts.Get<Chat>().BroadcastCollision(GetPlayers());
                    }
                }
                break;
        }
    }

    /*
     * Handles the fighter's attacks and locked on missiles.
     */
    public void Update()
    {
        if (GameManager.GameOver || Entity.IsDestroyed)
        {
            return;
        }

        m_velocity = m_rigidBody.Velocity;

        // progress timers and other time dependant properties
        m_laserReload.Update();
        m_missileReload.Update();
        m_missileLock.Update();
        m_outOfBounds.Update();

        LaserReload = ksMath.Clamp01(m_laserReload.Time / m_laserReloadTime + (m_laserReload.IsDone && !CanUseEnergy() ? 1 : 0));
        MissileReload = ksMath.Clamp01(m_missileReload.Time / m_missileReloadTime + (m_missileReload.IsDone && !CanUseEnergy() ? 1 : 0));

        Energy = Math.Min(Energy + m_energyRechargeRate * Time.Delta, m_maxEnergy);

        // makes the energy avaliable to use again after enough is accumulated
        EnergyFrozen = EnergyFrozen && Energy < m_energyFreezeEnd;

        // check if we are out of bounds. if we are, start a countdown and destroy us at the end if we are still out of bounds
        IsOutOfBounds = Entity.Transform.Position.Magnitude() > m_outOfBoundsRadius;

        if (!IsOutOfBounds)
        {
            m_outOfBounds.Stop();
        }
        else if (IsOutOfBounds && !m_outOfBounds.IsActive)
        {
            m_outOfBounds.Start();
        }
        else if (m_outOfBounds.IsActive && m_outOfBounds.IsDone)
        {
            FighterDestroyed(false);
            Room.Scripts.Get<Chat>().BroadcastOutOfBounds(GetPlayers());
        }

        // keep track of missiles locked on to the fighter, and if any of them don't exist anymore, then remove it from our list of locked on missiles
        ArrayList removeMissiles = new ArrayList();
        foreach (ksIServerEntity missile in m_incomingMissiles)
        {
            if (Room.GetEntity(missile.Id) != missile)
            {
                removeMissiles.Add(missile);
            }
        }
        foreach (ksIServerEntity missile in removeMissiles)
        {
            m_incomingMissiles.Remove(missile);
        }
        IsTargeted = m_incomingMissiles.Count > 0;


        // firing weapons
        m_missileLockTarget = null;
        FighterController controller = (FighterController)Entity.PlayerController;
        if (controller != null)
        {
            //Shields
            if (controller.UseShield && ShieldCount > 0 && m_shieldTime <= 0.0f)
            {
                m_shieldTime = 5.0f;
                ShieldCount--;
                UsingShield = controller.UseShield;
            }

            if (m_shieldTime > 0.0f)
            {
                m_shieldTime -= Time.Delta;
            }
            else if (UsingShield)
            {
                UsingShield = false;
                ShieldReload = 1;
            }

            // Boost
            BoostReload = CanUseEnergy() ? 0 : 1;
            if (controller.UseBoost && CanUseEnergy())
            {
                SpendEnergy(m_boostEnergyDrain * Time.Delta);
            }

            // Lasers
            if (controller.Laser && m_laserReload.IsDone && CanUseEnergy())
            {
                m_laserShootSide *= -1;
                m_laserReload.Start();
                ksVector3 pos = Transform.ToWorld(new ksVector3(
                    m_laserShootSide * m_laserSpawnPosition.X, m_laserSpawnPosition.Y, m_laserSpawnPosition.Z));
                ksQuaternion rot = ksQuaternion.FromDirection(m_crosshairTarget - pos);
                Laser laser = Room.SpawnEntity(ID.TYPE.LASER, pos, rot, ksVector3.One).Scripts.Get<Laser>();
                laser.InitializeLaser(m_player, m_laserSpeed);
                SpendEnergy(laser.EnergyCost);
            }

            // Missiles
            if (MissileCount > 0 && controller.Missile && m_missileReload.IsDone && CanUseEnergy())
            {
                // finds an enemy target that is closest to the direction of the fighter
                float largestDot = m_missileMinDot;
                foreach (Fighter fighter in m_instances)
                {
                    if (fighter != this && !fighter.Entity.IsDestroyed && (Team == null || fighter.Team != Team))
                    {
                        ksVector3 targetDirection = (fighter.Transform.Position - Transform.Position).Normalized();
                        float dot = ksVector3.Dot(Transform.Forward(), targetDirection);
                        if (dot > largestDot)
                        {
                            m_missileLockTarget = fighter.Entity;
                            largestDot = dot;
                        }
                    }
                }

                // if the fighter has been locked for long enough, fire the missile at the target
                if (m_missileLockTarget != null && m_missileLock.IsDone)
                {
                    m_missileReload.Start();
                    ksVector3 spawnPosition = Transform.ToWorld(m_missileSpawnPosition);

                    MissileCount--;
                    Missile missile = Room.SpawnEntity(ID.TYPE.MISSILE, spawnPosition, Entity.Transform.Rotation, ksVector3.One)
                        .Scripts.Get<Missile>();
                    missile.InitializeMissile(m_missileLockTarget, m_player);
                    SpendEnergy(missile.EnergyCost);

                    Fighter fs = m_missileLockTarget.Scripts.Get<Fighter>();
                    if (fs != null)
                    {
                        fs.AddIncomingMissile(missile.Entity);
                    }
                }
            }
        }
        else
        {
            UsingShield = false;
        }

        // if we are targeting an enemy, record the id of the target so we can send it to the client
        if (m_missileLockTarget != null)
        {
            LockTarget = m_missileLockTarget.Id;
        }
        else // otherwise clear our target
        {
            LockTarget = 0;
            m_missileLock.Start();
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
     * Lowers the fighte's energy pool if avaliable and checks if the fighter ran out of energy,
     * temporarily freezing its use. Returns wether or not the energy cost was able to be afforded.
     */
    private bool SpendEnergy(float energyCost)
    {
        if (Energy >= 0 && !EnergyFrozen)
        {
            Energy -= energyCost;
            EnergyFrozen = Energy < 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
     * When the fighter is destroyed, decrease the owner's score and inform them that they need a new fighter.
     */
    private void FighterDestroyed(bool turretLosePoints)
    {
        if (Entity.PlayerController != null)
        {
            Entity.Owner.Scripts.Get<FighterPlayer>().FighterDestroyed();
        }

        m_turret.TurretDestroyed(turretLosePoints);
        Physics.ApplyExplosiveForce(Transform.Position, 3, 50, ksMath.Interpolation.EASE_OUT_SIN);
        Entity.Destroy();
    }

    /*
     * Takes damage from an enemy weapon.
     */
    public void Hit(ksIServerEntity projectile, int damage, Player projectileOwner)
    {
        if (projectileOwner == m_player || projectileOwner == m_turret.GetPlayer() || (Team != null && projectileOwner.Team == Team))
        {
            return;
        }

        Laser laser = projectile.Scripts.Get<Laser>();
        if (laser != null)
        {
            laser.PlinkSound();
        }

        projectile.Destroy();

        if (UsingShield && Energy > 0)
        {
            Health -= (int)(damage * m_shieldDamageFraction);
        }
        else
        {
            Health -= damage;
        }

        Entity.CallRPC(ID.RPC.FIGHTER_HIT);

        if (Health <= 0)
        {
            int kills = m_turret.IsOccupied ? 2 : 1;
            projectileOwner.IncrementKills(kills, m_scoreValue * kills);

            Room.Scripts.Get<Chat>().BroadcastKill(projectileOwner, GetPlayers());
            FighterDestroyed(true);
        }
    }

    [ksRPC(ID.RPC.FIGHTER_TARGET)]
    public void SetCrosshairTarget(ksIServerPlayer player, ksVector3 crosshairTarget)
    {
        if (player != Entity.Owner || Entity.Owner == null)
        {
            return;
        }
        m_crosshairTarget = crosshairTarget;
    }

    private void SpawnCrates()
    {
        if (GameManager.GameOver)
        {
            return;
        }
        ksVector3 position;
        ksVector3 velocity;
        ksVector3 angularVelocity;
        ksQuaternion rotation;

        for (int i = 0; i<ShieldCount; i++)
        {
            velocity = m_rand.NextUnitVector3();
            position = Entity.Transform.Position + velocity;
            angularVelocity = m_rand.NextVector3();
            rotation = m_rand.NextQuaternion();

            ksIServerEntity crate = Room.SpawnEntity(ID.TYPE.SHIELD_CRATE, position, rotation, ksVector3.One);
            ksRigidBody rigidBody = crate.Scripts.Get<ksRigidBody>();
            rigidBody.Velocity = velocity * m_rand.NextFloat(0.0f, 5.0f);
            rigidBody.AngularVelocity = angularVelocity;
        }

        for (int i = 0; i < MissileCount; i++)
        {
            velocity = m_rand.NextUnitVector3();
            position = Entity.Transform.Position + velocity;
            angularVelocity = m_rand.NextVector3();
            rotation = m_rand.NextQuaternion();

            ksIServerEntity crate = Room.SpawnEntity(ID.TYPE.MISSILE_CRATE, position, rotation, ksVector3.One);
            ksRigidBody rigidBody = crate.Scripts.Get<ksRigidBody>();
            rigidBody.Velocity = velocity * m_rand.NextFloat(0.0f, 5.0f);
            rigidBody.AngularVelocity = angularVelocity;
        }

        Room.SpawnEntity(ID.TYPE.HEALTH_CRATE, Entity.Transform.Position, Entity.Transform.Rotation, ksVector3.One);
    }

}