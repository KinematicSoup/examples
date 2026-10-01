using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using KS.Reactor.Server;
using KS.Reactor;

// Handles player spawning
public class srPlayerSpawner : ksServerRoomScript
{
    [ksEditable]
    public ksPlayerController Controller;

    [ksEditable]
    public string Prefab = "Character";

    [ksEditable]
    public int NumBots = 0;

    [ksEditable]
    public bool LoadTestBotsUnlimitedAmmo = true;

    private ksAtomicHashSet<uint> m_loadTestBots = new ksAtomicHashSet<uint>();
    private srEventQueue m_eventQueue;
    private ksSweepParams m_sweepArgs = new ksSweepParams();
    private srLevelGenerator m_levelGenerator;

    public ksRandom Rand
    {
        get { return m_rand; }
    }
    private ksRandom m_rand = new ksRandom();

    // Called when the script is attached.
    public override void Initialize()
    {
        m_levelGenerator = Scripts.Get<srLevelGenerator>();

        m_sweepArgs.Direction = ksVector3.Down;
        m_sweepArgs.Distance = 100f;
        m_eventQueue = Scripts.Get<srEventQueue>();

        Room.OnPlayerJoin += PlayerJoin;
        Room.OnPlayerLeave += PlayerLeave;
        Room.OnAuthenticate += Authenticate;
        for (int i = 0; i < NumBots; i++)
        {
            Room.CreateVirtualPlayer("Bot" + i);
        }
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnPlayerJoin -= PlayerJoin;
        Room.OnPlayerLeave -= PlayerLeave;
    }

    // There should be one connection argument which is the player's username.
    private Task<ksAuthenticationResult> Authenticate(ksIServerPlayer player, ksMultiType[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1)
        {
            return Task.FromResult(new ksAuthenticationResult(1));
        }
        string username = null;
        if (args[0].Type == ksMultiType.Types.BOOL && args[0].Bool)
        {
            m_loadTestBots.Add(player.Id);
        }
        else
        {
            username = args[0];
        }
        if (username != null)
        {
            username = username.Trim();
        }
        if (string.IsNullOrEmpty(username) || username.Length > 20)
        {
            lock (m_rand)
            {
                username = "Combatant" + m_rand.Next(10) + m_rand.Next(10) + m_rand.Next(10);
            }
        }
        player.Properties[Prop.USERNAME] = username;
        return Task.FromResult(new ksAuthenticationResult(0));
    }

    // Called when a player connects.
    private void PlayerJoin(ksIServerPlayer player)
    {
        SpawnPlayer(player);
    }
    
    // Called when a player disconnects.
    private void PlayerLeave(ksIServerPlayer player)
    {
        ksLog.Info(player.Properties[Prop.USERNAME] + " left.");
        player.DestroyControlledEntities();
        m_loadTestBots.Remove(player.Id);
    }

    public void SpawnPlayer(ksIServerPlayer player)
    {
        ksIServerEntity entity = Room.SpawnEntity(Prefab);
        entity.Transform.Position = GetSpawnPoint(entity);
        if (Controller == null)
        {
            return;
        }
        seCharacter character = entity.Scripts.Get<seCharacter>();
        entity.SetController(Controller.Clone(), player);
        FPSController controller = entity.PlayerController as FPSController;
        if (controller != null)
        {
            // Register controller callbacks. Actions that must be done on the main thread are queued in the event
            // queue.
            controller.Collider = entity.Scripts.Get<ksCapsuleCollider>();
            controller.OnCollide = OnCollision;
            controller.OnPickUp = (ksIEntity e) =>
            {
                sePickUp pickUp = ((ksIServerEntity)e).Scripts.Get<sePickUp>();
                if (pickUp != null)
                {
                    m_eventQueue.Enqueue(() => { pickUp.HandlePickUp(character); });
                }
            };
            controller.OnCrush = () => { m_eventQueue.Enqueue(entity.Destroy); };
            controller.GetGroundVelocity = GetGroundVelocity;
            controller.IsDestroyed = () => entity.IsDestroyed;
        }
        if (LoadTestBotsUnlimitedAmmo)
        {
            character.UnlimitedAmmo = m_loadTestBots.Contains(player.Id);
        }
        character.EquipWeapon(character.WeaponIndex);
    }

    // Get a random spawn point
    public ksVector3 GetSpawnPoint(ksIServerEntity entity)
    {
        if (m_levelGenerator == null)
        {
            return new ksVector3(0f, 5f, 0f);
        }
        ksVector3 position = ksVector3.Zero;
        float min = 1f - World.TILE_SIZE / 2f;
        float max = min + World.TILE_SIZE * m_levelGenerator.GridSize - 2f;
        lock (m_rand)
        {
            position.X = m_rand.NextFloat(min, max);
            position.Z = m_rand.NextFloat(min, max);
        }

        m_sweepArgs.Entity = entity;
        m_sweepArgs.ExcludeEntity = entity;
        m_sweepArgs.Origin = position + ksVector3.Up * m_sweepArgs.Distance;
        ksSweepResult hit;
        if (Physics.SweepNearest(m_sweepArgs, out hit))
        {
            position = hit.Point;
        }

        position.Y += 5f;
        return position;
    }

    [ksRPC(RPC.SWITCH_WEAPON)]
    private void SwitchWeapon(ksIServerPlayer player)
    {
        if (player.ControlledEntities.Count > 0)
        {
            seCharacter character = player.ControlledEntities[0].Scripts.Get<seCharacter>();
            character.SwitchWeapon();
        }
    }

    private ksVector3 GetGroundVelocity(ksSweepResult hit)
    {
        ksIServerEntity entity = (ksIServerEntity)hit.Entity;
        sePlatform platform = entity.Scripts.Get<sePlatform>();
        if (platform != null)
        {
            return platform.Velocity;
        }
        ksRigidBody rigidBody = entity.Scripts.Get<ksRigidBody>();
        if (rigidBody != null && !rigidBody.IsKinematic)
        {
            return rigidBody.Velocity + Utils.CalculateVelocityFromRotation(hit, rigidBody.AngularVelocity);
        }
        return ksVector3.Zero;
    }

    private void OnCollision(FPSController controller, ksSweepResult hit)
    {
        ksIServerEntity entity = (ksIServerEntity)hit.Entity;
        if (entity.Transform.IsPermanent)
        {
            return;
        }
        // Apply an impulse to rigid bodies the character runs into.
        ksRigidBody rigidBody = entity.Scripts.Get<ksRigidBody>();
        if (rigidBody != null && !rigidBody.IsKinematic)
        {
            ksVector3 pointVelocity = rigidBody.Velocity +
                Utils.CalculateVelocityFromRotation(hit, rigidBody.AngularVelocity);
            ksVector3 dv = controller.Velocity - pointVelocity;
            ksVector3 impulse = ksVector3.Project(dv, hit.Normal);
            m_eventQueue.Enqueue(() =>
            {
                rigidBody.AddForceAtPosition(impulse * controller.ImpulseMultiplier, hit.Point, ksForceMode.IMPULSE);
            });
        }
    }

    // Load test bots don't decode and don't know which entity is theirs, so they call a Room RPC to shoot instead of
    // an entity RPC.
    [ksRPC(RPC.SHOOT)]
    private void OnShoot(ksIServerPlayer player, ksMultiType[] args)
    {
        if (m_loadTestBots.Contains(player.Id) && player.ControlledEntities.Count > 0)
        {
            ksIServerEntity entity = player.ControlledEntities[0];
            if (args.Length == 0)
            {
                entity.OnRPC[RPC.SHOOT].Execute(player);
            }
            else
            {
                ulong frame = args[0];
                frame = Math.Max(frame, Time.Frame - 5);
                entity.OnRPC[RPC.SHOOT].Execute(player, entity.Transform.Position, frame, 0);
            }
        }
    }
}