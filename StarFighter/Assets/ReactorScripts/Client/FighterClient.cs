using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;

public class FighterClient : ksEntityScript
{
    [Tooltip("The explosion prefab instantaited upon the fighter's destuction.")]
    public Transform Explosion;

    [Tooltip("The prefab instantiated when the fighter is hit by a projectile.")]
    public Transform HitSound;

    private PlayerClient m_player;

    public string PlayerName
    {
        get { return m_player.PlayerName; }
    }

    public int TeamNumber
    {
        get { return m_player.TeamNumber; }
    }

    public int Health
    {
        get { return Properties[ID.PROP.FIGHTER.HEALTH]; }
    }

    public float Energy
    {
        get { return Properties[ID.PROP.FIGHTER.ENERGY]; }
    }

    public bool EnergyFrozen
    {
        get { return Properties[ID.PROP.FIGHTER.ENERGY_FREEZE]; }
    }

    public bool UsingShield
    {
        get { return Properties[ID.PROP.FIGHTER.USING_SHIELD]; }
    }

    public float LaserReloadProgress
    {
        get { return Properties[ID.PROP.FIGHTER.LASER_RELOAD]; }
    }

    public float MissileReloadProgress
    {
        get { return Properties[ID.PROP.FIGHTER.MISSILE_RELOAD]; }
    }

    public float BoostReloadProgress
    {
        get { return Properties[ID.PROP.FIGHTER.BOOST_RELOAD]; }
    }

    public float ShieldReloadProgress
    {
        get { return Properties[ID.PROP.FIGHTER.SHIELD_RELOAD]; }
    }

    private uint LockTarget
    {
        get { return Properties[ID.PROP.FIGHTER.LOCK_TARGET]; }
    }

    public bool IsTargeted
    {
        get { return Properties[ID.PROP.FIGHTER.IS_TARGETED]; }
    }

    public bool OutOfBounds
    {
        get { return Properties[ID.PROP.FIGHTER.OUT_OF_BOUNDS]; }
    }

    public int MissileCount
    {
        get { return Properties[ID.PROP.FIGHTER.MISSILE_COUNT]; }
    }

    public int MissileCapacity
    {
        get { return Properties[ID.PROP.FIGHTER.MISSILE_CAPACITY]; }
    }

    public int ShieldCount
    {
        get { return Properties[ID.PROP.FIGHTER.SHIELD_COUNT]; }
    }

    public int ShieldCapacity
    {
        get { return Properties[ID.PROP.FIGHTER.SHIELD_CAPACITY]; }
    }

    public override void Initialize()
    {
        Entity.OnDestroy += Destroy;

        m_player = Room.GetPlayer(Entity.OwnerId).GameObject.GetComponent<PlayerClient>();
        if (m_player != null)
        {
            m_player.PlayerEntity = Entity;
        }
        SetFighterMaterials();

        if (Entity.PlayerController != null)
        {
            gameObject.tag = "Player";
            PlayerClient.LocalPlayerType = PlayerType.FIGHTER;
        }
        else
        {
            uint localPlayerTeam = Room.LocalPlayer.Properties[ID.PROP.PLAYER.TEAM_NUMBER];
            GetComponent<PositionIndicator>().Color = localPlayerTeam < 0 || localPlayerTeam != TeamNumber ?
                Color.red : Color.green;
        }
    }

    public override void Detached()
    {
        Entity.OnDestroy -= Destroy;
    }

    /*
     * called when the fighter is destroyed
     */
    private void Destroy(ksDestroyReason reason)
    {
        if (reason == ksDestroyReason.SERVER_DESTROY)
        {
            if (Camera.main.transform.root == transform)
            {
                Camera.main.transform.parent = null;
            }

            // disconnects temporary sound source like the hit sound to prevent cutting the clip short
            foreach (Transform t in transform.Find("TempSounds"))
            {
                t.SetParent(null);
            }

            Instantiate(Explosion, Entity.Transform.Position, Entity.Transform.Rotation);
        }
    }

    /*
     * plays a hit sound if the fighter has lost health
     */
    [ksRPC(ID.RPC.FIGHTER_HIT)]
    private void PlayHitSound()
    {
        Transform t = Instantiate(HitSound, transform.position, Quaternion.identity) as Transform;
        t.SetParent(transform.Find("TempSounds"));
    }

    /*
     * is this fighter locking onto an enemy fighter
     */ 
    public bool IsTargeting()
    {
        return LockTarget != 0;
    }

    /*
     * returns the position of the entity being targeted if applicable
     */ 
    public Vector3 GetMissileTarget()
    {
        if (IsTargeting())
        {
            return Room.GetEntity(LockTarget).Transform.Position;
        }
        return Vector3.zero;
    }

    /*
     * sets the materials of the fighter to use the custom and team color
     */ 
    private void SetFighterMaterials()
    {
        Material coloredMat = GetComponent<MeshRenderer>().material;

        coloredMat.SetColor("_TeamColor", m_player.Color);
        coloredMat.SetColor("_EmissionColor", m_player.Color);

        Transform turret = transform.Find("turret");
        Transform barrels = turret.Find("barrels");

        GetComponent<MeshRenderer>().sharedMaterial = coloredMat;
        turret.GetComponent<MeshRenderer>().sharedMaterial = coloredMat;
        barrels.GetComponent<MeshRenderer>().sharedMaterial = coloredMat;
    }

    public void SetCrosshairTarget(Vector3 crosshairTarget)
    {
        Entity.CallRPC(ID.RPC.FIGHTER_TARGET, crosshairTarget);
    }
}