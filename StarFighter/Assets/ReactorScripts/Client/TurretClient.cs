using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;

public class TurretClient : ksEntityScript
{
    [Tooltip("The prefab used as the players's crosshair.")]
    public Transform Crosshair;

    private uint PlayerId
    {
        get { return Properties[ID.PROP.TURRET.OWNER]; }
    }

    public string PlayerName
    {
        get { return m_player == null ? "" : m_player.PlayerName; }
    }

    public float Energy
    {
        get { return Properties[ID.PROP.TURRET.ENERGY]; }
    }

    public bool EnergyFrozen
    {
        get { return Properties[ID.PROP.TURRET.ENERGY_FREEZE]; }
    }

    public float LaserReloadProgress
    {
        get { return Properties[ID.PROP.TURRET.LASER_RELOAD]; }
    }

    public float Rotation
    {
        get
        {
            if (!IsOccupied)
            {
                return 0f;
            }
            ksVector3 direction = Properties[ID.PROP.TURRET.DIRECTION];
            ksVector3 localDirection = Entity.Transform.Rotation.Inverse() * direction;
            ksVector3 euler = ksQuaternion.FromDirection(localDirection, Entity.Transform.Up()).ToEuler();
            return euler.Y;
        }
    }

    public float Elevation
    {
        // changes range from [0, 360) to [-180, 180)
        get
        {
            if (!IsOccupied)
            {
                return 0f;
            }
            ksVector3 direction = Properties[ID.PROP.TURRET.DIRECTION];
            ksVector3 localDirection = Entity.Transform.Rotation.Inverse() * direction;
            ksVector3 euler = ksQuaternion.FromDirection(localDirection, Entity.Transform.Up()).ToEuler();
            float elevation = euler.X;
            return elevation < 180.0f ? elevation : elevation - 360.0f;
        } 
    }


    private Transform m_turret;
    private Transform m_barrels;
    private PlayerClient m_player;

    public bool IsOwner
    {
        get { return PlayerId == Room.LocalPlayerId; }
    }

    public bool IsOccupied
    {
        get { return PlayerId != 0; }
    }

    /**
     * Called after properties are initialized.
     */
	public override void Initialize()
	{
        Entity.OnPropertyChange[ID.PROP.TURRET.OWNER] += ReceivePlayer;

        m_turret = transform.Find("turret");
        m_barrels = m_turret.Find("barrels");

        SetPlayer();
	}

    /**
     * Called when the script is detached.
     */
    public override void Detached()
    {
        Entity.OnPropertyChange[ID.PROP.TURRET.OWNER] -= ReceivePlayer;
        if (IsOwner)
        {
            Camera.main.transform.SetParent(null);
        }
    }

    /*
     * the ownership of the turret has changed, so change it.
     */
    private void ReceivePlayer(ksMultiType oldVal, ksMultiType newVal)
    {
        SetPlayer();
    }

    /*
     * sets whether this turret is controlled by the local player or not
     */
    private void SetPlayer()
    {
        ksPlayer player = Room.GetPlayer(PlayerId);
        m_player = player == null ? null : player.GameObject.GetComponent<PlayerClient>();
        if (m_player != null)
        {
            m_player.PlayerEntity = Entity;
        }

        if (Room.LocalPlayer != null && PlayerId == Room.LocalPlayer.Id)
        {
            gameObject.tag = "Player";
            PlayerClient.LocalPlayerType = PlayerType.TURRET;

            Transform crosshair_t = Instantiate(Crosshair);
            crosshair_t.SetParent(m_barrels, false);
            crosshair_t.localPosition = Vector3.forward * 1000;
            crosshair_t.localRotation = Quaternion.identity;
            crosshair_t.localScale = Vector3.one * 50;
        }
        else if (PlayerClient.LocalPlayerType == PlayerType.TURRET)
        {
            gameObject.tag = "Untagged";

            if (m_barrels.Find(Crosshair.name))
            {
                Destroy(m_barrels.Find(Crosshair.name));
            }
        }
    }

    /**
     * Called every frame.
     */
	public void Update()
	{
        m_turret.localRotation = Quaternion.Euler(new Vector3(0, Rotation, 0));
        m_barrels.localRotation = Quaternion.Euler(new Vector3(Elevation, 0, 0));
	}

    public void FireLaser()
    {
        Entity.CallRPC(ID.RPC.TURRET_LASER);
    }
    
    public void ChangeElevation(float delta)
    {
        if (delta != 0f)
        {
            Entity.CallRPC(ID.RPC.TURRET_ELEVATION, delta);
        }
    }

    public void ChangeRotation(float delta)
    {
        if (delta != 0f)
        {
            Entity.CallRPC(ID.RPC.TURRET_ROTATION, delta);
        }
    }
}