using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

// Server character script
[ksSharedData]
public class seCharacter : ksServerEntityScript
{
    [ksEditable]
    public int MaxHealth = 100;
    // The gun shoots from this high above the player's origin.
    [ksEditable]
    public float GunOffsetY = 1.5f;
    [ksEditable]
    public sWeapon[] Weapons;
    // Index of the equipped weapon.
    [ksEditable]
    public int WeaponIndex = 0;

    public bool UnlimitedAmmo = false;

    [ksEditable]
    public int Bullets
    {
        get { return GetAmmo(AmmoTypes.BULLETS); }
        set { SetAmmo(AmmoTypes.BULLETS, value); }
    }

    [ksEditable]
    public int MaxBullets
    {
        get { return m_maxAmmo[(int)AmmoTypes.BULLETS]; }
        set { m_maxAmmo[(int)AmmoTypes.BULLETS] = value; }
    }

    [ksEditable]
    public int Grenades
    {
        get { return GetAmmo(AmmoTypes.GRENADES); }
        set { SetAmmo(AmmoTypes.GRENADES, value); }
    }

    [ksEditable]
    public int MaxGrenades
    {
        get { return m_maxAmmo[(int)AmmoTypes.GRENADES]; }
        set { m_maxAmmo[(int)AmmoTypes.GRENADES] = value; }
    }

    private int[] m_ammo = new int[AmmoConsts.NUM_TYPES];
    private int[] m_maxAmmo = new int[AmmoConsts.NUM_TYPES];
    private sWeapon m_weapon;

    public delegate void DamageHandler(seCharacter victim, ksIServerPlayer attacker, int damage);
    public static event DamageHandler OnDamage;

    public delegate void DeathHandler(seCharacter character);
    public static event DeathHandler OnDeath;

    public int Health
    {
        get { return Properties[Prop.HEALTH]; }
        set
        {
            if (value <= 0)
            {
                Properties[Prop.HEALTH] = 0;
                Entity.Destroy();
                return;
            }
            value = Math.Min(value, MaxHealth);
            Properties[Prop.HEALTH] = value;
        }
    }

    public seCharacter()
    {
        // Set defaults
        MaxBullets = 100;
        MaxGrenades = 50;
        Bullets = 40;
        Grenades = 20;
    }

    // Called when the script is attached.
    public override void Initialize()
    {
        Health = MaxHealth;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        // If the player did not disconnect and the round isn't over (time scale > 0).
        if (Entity.Owner != null && Entity.Owner.Connected && Time.TimeScale > 0f)
        {
            // Spawn a health pickup where the player died and start the player's respawn timer.
            Room.SpawnEntity("Health", Transform.Position + ksVector3.Up * 1.5f);
            Entity.Owner.Scripts.Get<spRespawn>().StartRespawnTimer();
            if (OnDeath != null)
            {
                OnDeath(this);
            }
        }
    }

    public void Equip(sWeapon weapon)
    {
        if (m_weapon != null)
        {
            m_weapon.Unequip();
        }
        m_weapon = weapon;
        FPSController controller = Entity.PlayerController as FPSController;
        if (controller != null)
        {
            controller.Weapon = weapon;
        }
        if (weapon != null)
        {
            weapon.Character = this;
            weapon.Equip();
            Properties[Prop.WEAPON] = weapon.AssetId;
        }
        else
        {
            Properties[Prop.WEAPON] = 0;
        }
    }

    public int GetAmmo(AmmoTypes type)
    {
        if (type == AmmoTypes.NONE)
        {
            return 0;
        }
        return m_ammo[(int)type];
    }

    public void SetAmmo(AmmoTypes type, int amount)
    {
        if (type == AmmoTypes.NONE)
        {
            return;
        }
        int index = (int)type;
        m_ammo[index] = ksMath.Clamp(amount, 0, m_maxAmmo[index]);
    }

    public void AddAmmo(AmmoTypes type, int amount)
    {
        if (type == AmmoTypes.NONE)
        {
            return;
        }
        int ammo = GetAmmo(type);
        SetAmmo(type, ammo + amount);
        int newAmmo = GetAmmo(type);
        if (newAmmo != ammo)
        {
            Entity.CallRPC(Entity.Owner, RPC.ADD_AMMO, (int)type, newAmmo - ammo);
        }
    }

    public void EquipWeapon(int index)
    {
        if (Weapons == null || Weapons.Length == 0)
        {
            return;
        }
        WeaponIndex = ksMath.Clamp(index, 0, Weapons.Length - 1);
        Equip((sWeapon)Weapons[WeaponIndex].Clone());
    }

    public void SwitchWeapon()
    {
        if (Weapons == null || Weapons.Length < 2)
        {
            return;
        }
        WeaponIndex++;
        WeaponIndex %= Weapons.Length;
        EquipWeapon(WeaponIndex);
    }

    public void ApplyDamage(int damage, ksVector3 point, ksIServerPlayer attacker = null)
    {
        if (Entity.IsDestroyed)
        {
            return;
        }
        if (!attacker.Connected || attacker == Entity.Owner)
        {
            attacker = null;
        }
        if (OnDamage != null)
        {
            OnDamage(this, attacker, damage);
        }

        Health -= damage;
        if (attacker == null)
        {
            return;
        }
        Room.CallRPC(attacker, RPC.DAMAGE, damage, point);
        if (Entity.IsDestroyed && Entity.Owner != null)
        {
            Room.CallRPC(attacker, RPC.KILL, Entity.Owner.Id);
            Room.CallRPC(Entity.Owner, RPC.KILLED, attacker.Id);
        }
    }
}