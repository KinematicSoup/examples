using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using KSProxies.Scripts;

// Client base class for guns that use ammo and have a cooldown before they can shoot again.
// Proxy is the proxy class generated for the corresponding server weapon asset.
public class AmmoGun<Proxy> : MonoBehaviour, IWeapon, IEntityReference where Proxy : sAmmoGun
{
    // The server weapon asset
    public Proxy ProxyAsset;
    // Flash affect when firing.
    public Fader Fader;
    // Sound to play when firing.
    public Sound ShootSound;
    // Sound to play when you try to fire with no ammo.
    public Sound NoAmmoSound;

    // The entity for the player using this gun.
    public ksEntity Entity
    {
        get { return m_entity; }
        set { m_entity = value; }
    }
    private ksEntity m_entity;

    protected ksClientTime m_time;
    protected FPSController m_controller;
    protected bool m_shoot = false;
    // The cooldown timer. The client and server both track this independantly. If a client tries to fire too early,
    // the server won't actually fire until its cooldown reaches zero.
    protected float m_timer = 0f;
    protected ceCharacter m_character;
    protected WeaponMovement m_weaponMovement;

    public int Ammo
    {
        get { return m_character.GetAmmo((AmmoTypes)ProxyAsset.AmmoType); }
        set { m_character.SetAmmo((AmmoTypes)ProxyAsset.AmmoType, value); }
    }

    protected virtual void Start()
    {
        if (m_entity == null)
        {
            return;
        }
        m_entity.OnRPC[RPC.SHOOT] += OnShoot;
        m_timer = ProxyAsset.Cooldown;
        m_time = m_entity.Room.Time;
        m_controller = m_entity.PlayerController as FPSController;
        if (m_controller != null)
        {
            // This is the local player.
            Hud.Instance.SetAmmoType((AmmoTypes)ProxyAsset.AmmoType);
            m_character = m_entity.GameObject.GetComponent<ceCharacter>();
            m_character.Weapon = this;
            m_weaponMovement = Camera.main.transform.GetChild(0).GetComponent<WeaponMovement>();
            m_entity.Room.OnRPC[RPC.ADD_TIME] += AddTime;
        }
        else
        {
            // Disable updates for the non-local player.
            enabled = false;
        }
    }

    protected virtual void OnDestroy()
    {
        m_entity.OnRPC[RPC.SHOOT] -= OnShoot;
        if (m_controller != null && m_entity.Room != null)
        {
            m_entity.Room.OnRPC[RPC.ADD_TIME] -= AddTime;
            if (Equals(m_character.Weapon))
            {
                m_character.Weapon = null;
            }
        }
    }

    // Shooting is done from late update so the FirstPersonCamera updates our aim before we shoot.
    protected virtual void LateUpdate()
    {
        // Do not fire while stunned or waiting for a weapon change.
        if (Entity.Properties[Prop.STUNNED] || m_character.WeaponChangePending)
        {
            return;
        }
        // Check if the cooldown timer is finished.
        if (m_timer > 0f)
        {
            m_shoot = false;
            m_timer -= m_time.AdjustedDelta;
            return;
        }
        if (!m_shoot)
        {
            return;
        }
        m_timer = ProxyAsset.Cooldown;
        if (Ammo <= 0)
        {
            SoundManager.Instance.Play(NoAmmoSound, gameObject);
            m_shoot = false;
            return;
        }
        Ammo--;
        // Show the gun flash
        Fader.Alpha = 1f;
        SoundManager.Instance.Play(ShootSound, gameObject);
        m_weaponMovement.Recoil();
        m_shoot = false;
        Shoot();
    }

    // Check if the shoot button is down.
    public void OnUpdate(ksInput input)
    {
        if (input.IsDown(Buttons.SHOOT))
        {
            m_shoot = true;
        }
    }

    // Called when the local player shoots. Derived classes should override this.
    protected virtual void Shoot()
    {

    }

    // Called when we receive an RPC from the server telling us the player shot.
    protected virtual void OnShoot(ksMultiType[] args)
    {
        // Do nothing for the local player because we already animated the shot.
        if (m_controller == null)
        {
            // Don't show flash if the character isn't visible.
            if (gameObject.activeInHierarchy)
            {
                Fader.Alpha = 1f;
            }
            SoundManager.Instance.Play(ShootSound, gameObject);
        }
    }

    // If our cooldown timer got too far out of sync with the server and we fired too soon, the server tells us to add
    // more time to the cool down timer.
    private void AddTime(ksMultiType[] args)
    {
        m_timer += args[0];
    }
}
