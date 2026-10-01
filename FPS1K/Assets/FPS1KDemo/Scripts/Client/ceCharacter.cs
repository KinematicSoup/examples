using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using KSProxies.Scripts;

// Client character script
public class ceCharacter : ksEntityScript
{
    // How long in seconds it takes to cross fade between animations.
    public float CrossFadeDuration = .2f;
    // How long in seconds the character flashes red when damaged.
    public float RedDuration = 1f / 3f;
    // How long in seconds the character leaves a ragdoll corpse behind after dieing.
    public float RagdollLifetime = 5f;
    // The object that becomes the root of the ragdoll when the player dies.
    public Transform RagdollRoot;
    // Transform that controls the player's aim.
    public Transform AimTarget;
    // The equipped weapon is attached to this object.
    public Transform WeaponSlot;

    // Make all characters invisible. This is to test the impact rendering/animating the characters has on the fps.
    [HideInInspector]
    public bool Invisible = false;

    private seCharacter m_proxy;
    private Animator m_animator;
    private Renderer m_renderer;
    private RectTransform m_minimapIcon;
    private WeaponMovement m_weaponMovement;
    private IWeapon m_weapon;
    private float m_red = 0f;
    private int m_pendingWeaponChanges;
    private bool m_isRagdoll = false;
    private bool m_fixAnimation = false;

    // How much ammo of each type the player has. Only tracked for the local player.
    private int[] m_ammo;
    private int[] m_maxAmmo;

    // Have we requested a weapon change and are waiting for a server response?
    public bool WeaponChangePending
    {
        get { return m_pendingWeaponChanges > 0; }
    }

    // The equipped weapon. Only set for the local player to get input updates.
    public IWeapon Weapon
    {
        get { return m_weapon; }
        set { m_weapon = value; }
    }

    // Called after properties are initialized.
    public override void Initialize()
    {
        Entity.OnDestroy += Destroyed;
        OnWeaponChange(0, Properties[Prop.WEAPON]);
        Entity.OnPropertyChange[Prop.WEAPON] += OnWeaponChange;
        Entity.OnPropertyChange[Prop.HEALTH] += OnHealthChange;

        m_proxy = GetComponent<seCharacter>();

        m_animator = GetComponent<Animator>();
        SetRagdoll(false);
        OnMoveStateChange(0, Properties[Prop.MOVE_STATE]);
        Entity.OnPropertyChange[Prop.MOVE_STATE] += OnMoveStateChange;

        if (Entity.PlayerController != null)
        {
            // The entity has a player controller. That means this entity is controlled by the local player.
            m_ammo = new int[AmmoConsts.NUM_TYPES];
            m_maxAmmo = new int[AmmoConsts.NUM_TYPES];
            m_maxAmmo[(int)AmmoTypes.BULLETS] = m_proxy.MaxBullets;
            m_maxAmmo[(int)AmmoTypes.GRENADES] = m_proxy.MaxGrenades;
            SetAmmo(AmmoTypes.BULLETS, m_proxy.Bullets);
            SetAmmo(AmmoTypes.GRENADES, m_proxy.Grenades);

            m_minimapIcon = Hud.Instance.Minimap.CreatePlayerIcon();
            Hud.Instance.HideDeathScreen();
            Hud.Instance.Minimap.SetPosition(m_minimapIcon, transform.position, Camera.main.transform.eulerAngles.y);
            Hud.Instance.HealthBar.SetHealth(1f);

            m_weaponMovement = Camera.main.transform.GetChild(0).GetComponent<WeaponMovement>();

            FPSController controller = Entity.PlayerController as FPSController;
            if (controller != null)
            {
                controller.OnLand = OnLand;
                controller.GetGroundVelocity = GetGroundVelocity;
                CapsuleCollider collider = GetComponent<CapsuleCollider>();
                if (collider != null)
                {
                    controller.Collider = new ksUnityCollider(collider);

                    ksConvergingInputPredictor predictor = Entity.Predictor as ksConvergingInputPredictor;
                    if (predictor != null)
                    {
                        // Configure the predictor to sweep the collider against objects it can collide with when
                        // repositioning the entity.
                        predictor.QueryObject = controller.Collider;
                        predictor.QueryFilter = new ksSimulationFilter();
                        predictor.QueryFlags |= ksQueryFlags.EXCLUDE_TOUCHES;
                    }
                }
            }

            gameObject.layer = Layers.LOCAL_PLAYER;

            FirstPersonCamera.Target = transform;
            FirstPersonCamera.Controller = controller;
            // Disable rendering and animation for the local player.
            Destroy(RagdollRoot.gameObject);
            m_animator.enabled = false;
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        else if (Invisible)
        {
            WeaponSlot.transform.parent = transform;
            Destroy(RagdollRoot.gameObject);
            m_animator.enabled = false;
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        else
        {
            m_renderer = GetComponentInChildren<Renderer>();
        }
    }

    private void Destroyed(ksDestroyReason destroyReason)
    {
        Entity.OnDestroy -= Destroyed;
        Entity.OnPropertyChange[Prop.MOVE_STATE] -= OnMoveStateChange;
        Entity.OnPropertyChange[Prop.WEAPON] -= OnWeaponChange;
        Entity.OnPropertyChange[Prop.HEALTH] -= OnHealthChange;
        if (Entity.PlayerController != null)
        {
            // Destroy the minimap icon and show the death screen when the local player dies.
            if (m_minimapIcon != null)
            {
                Destroy(m_minimapIcon.gameObject);
            }
            if (Room.OwnedEntities.Count == 1)
            {
                Hud.Instance.HealthBar.SetHealth(0f);
                Hud.Instance.ShowDeathScreen();
            }
            if (m_weaponMovement != null)
            {
                m_weaponMovement.PlaybackSpeed = 0f;
            }
        }
        // We unchecked DestroyWithServer on the ksEntityComponent so the gameobject won't be destroyed until we
        // destroy it. If the character is a ragdoll corpse, we won't destroy it yet.
        if (!m_isRagdoll)
        {
            Destroy(gameObject);
        }
    }

    // Called every frame.
    public void Update()
    {
        if (Entity.PlayerController != null)
        {
            // This is the local player. Update player position on the minimap.
            Hud.Instance.Minimap.SetPosition(m_minimapIcon, transform.position, Camera.main.transform.eulerAngles.y);
            // Change weapons when F is pressed.
            if (Input.GetKeyDown(KeyCode.F))
            {
                m_pendingWeaponChanges++;
                Room.CallRPC(RPC.SWITCH_WEAPON);
            }
            if (m_weapon != null)
            {
                m_weapon.OnUpdate(Entity.PlayerController.Input);
            }
            return;
        }
        if (m_isRagdoll)
        {
            return;
        }

        if (m_fixAnimation)
        {
            m_animator.Play(GetAnimationName(Properties[Prop.MOVE_STATE]));
            m_fixAnimation = false;
        }
        bool wasVisible = m_animator.enabled;
        m_animator.enabled = ViewChecker.IsVisible(m_renderer);
        m_renderer.enabled = m_animator.enabled;
        RagdollRoot.gameObject.SetActive(m_animator.enabled);
        if (!wasVisible && m_animator.enabled)
        {
            // Unity doesn't let you change the animation while the animator is disabled or on the frame it becomes
            // enabled, so we do it on the next frame.
            m_fixAnimation = true;
        }

        if (AimTarget != null && !Entity.IsDestroyed && m_animator.enabled)
        {
            AimTarget.position = transform.position + ksVector3.Up * m_proxy.GunOffsetY + new ksVector3(0f, 0f, 5f) *
                Utils.GetAimRotation(Properties[Prop.AIM]);
        }
        if (m_renderer != null && m_red > 0f && RedDuration > 0f)
        {
            m_red -= Time.Delta / RedDuration;
            m_red = Math.Max(0f, m_red);
            m_renderer.material.color = new Color(1f, 1f - m_red, 1f - m_red);
        }
    }

    // Get the amount of ammo of the given type. Only works for the local player.
    public int GetAmmo(AmmoTypes type)
    {
        if (type == AmmoTypes.NONE || m_ammo == null)
        {
            return 0;
        }
        return m_ammo[(int)type];
    }

    // Set the amount of ammo of the given type. Only works for the local player.
    public void SetAmmo(AmmoTypes type, int amount)
    {
        if (type == AmmoTypes.NONE || m_ammo == null)
        {
            return;
        }
        m_ammo[(int)type] = amount;
        Hud.Instance.SetAmmo(type, amount, m_maxAmmo[(int)type]);
    }

    private void OnMoveStateChange(ksMultiType oldValue, ksMultiType newValue)
    {
        if (m_weaponMovement != null)
        {
            m_weaponMovement.PlaybackSpeed = GetPlaybackSpeed(newValue);
        }
        else if (m_animator != null && m_animator.enabled)
        {
            m_animator.CrossFade(GetAnimationName(newValue), CrossFadeDuration);
        }
    }

    private string GetAnimationName(uint moveState)
    {
        switch (moveState)
        {
            default:
            case MoveStates.IDLE:
            {
                return "Aim";
            }
            case MoveStates.SPRINT:
            case MoveStates.FORWARD:
            {
                return "Forward";
            }
            case MoveStates.BACKWARDS:
            {
                return "Backwards";
            }
        }
    }

    // Get the animation plackback speed for a movement state.
    private float GetPlaybackSpeed(uint moveState)
    {
        switch (moveState)
        {
            default:
            case MoveStates.IDLE:
            {
                return 0f;
            }
            case MoveStates.FORWARD:
            {
                return 1f;
            }
            case MoveStates.SPRINT:
            {
                return 8f / 5f;
            }
            case MoveStates.BACKWARDS:
            {
                return -1f;
            }
        }
    }

    private void OnLand()
    {
        SoundManager.Instance.Play(m_weaponMovement.FootStep);
    }

    private void OnWeaponChange(ksMultiType oldValue, ksMultiType newValue)
    {
        if (m_pendingWeaponChanges > 0)
        {
            m_pendingWeaponChanges--;
        }
        Transform slot = Entity.PlayerController == null ? WeaponSlot : Camera.main.transform.GetChild(0);
        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }
        GameObject weapon = WeaponCache.Get(newValue);
        weapon.transform.parent = slot;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        foreach (IEntityReference component in weapon.GetComponents<IEntityReference>())
        {
            component.Entity = Entity;
        }
        if (Entity.PlayerController != null)
        {
            // Prevent the local player's weapon from casting a shadow.
            foreach (MeshRenderer renderer in weapon.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }
    }

    private void OnHealthChange(ksMultiType oldValue, ksMultiType newValue)
    {
        if (newValue < oldValue)
        {
            if (Entity.PlayerController == null)
            {
                if (Invisible)
                {
                    return;
                }
                // Non-local players
                m_red = 1f;
                if (newValue <= 0 && m_animator.enabled)
                {
                    // When the player dies, the character becomes a ragdoll that exists only on the client.
                    SetRagdoll(true);
                    RagdollRoot.gameObject.AddComponent<Lifetime>().Duration = RagdollLifetime;
                }
            }
            else
            {
                // Local player
                Hud.Instance.HealthBar.SetHealth(newValue.Float / m_proxy.MaxHealth);
                Hud.Instance.IndicateDamage();
                Hud.Instance.TextOverlay.Add(oldValue - newValue, new Vector2(.5f, .25f), Color.red);
            }
        }
        else if (Entity.PlayerController != null)
        {
            // Local player
            Hud.Instance.HealthBar.SetHealth(newValue.Float / m_proxy.MaxHealth);
        }
    }

    [ksRPC(RPC.ADD_AMMO)]
    private void AddAmmo(int type, int amount)
    {
        if (m_ammo != null)
        {
            SetAmmo((AmmoTypes)type, m_ammo[type] + amount);
        }
    }

    [ksRPC(RPC.SHOOT)]
    private void Shoot(ksMultiType[] args)
    {
        // Do nothing. This function is only here to prevent getting warnings when the server calls this RPC before
        // the weapon has registered a shoot RPC handler.
    }

    private ksVector3 GetGroundVelocity(ksSweepResult hit)
    {
        ksEntity entity = (ksEntity)hit.Entity;
        ceVelocityTracker tracker = entity.GameObject.GetComponent<ceVelocityTracker>();
        if (tracker != null)
        {
            return tracker.Velocity + Utils.CalculateVelocityFromRotation(hit, tracker.AngularVelocity);
        }
        return ksVector3.Zero;
    }

    private void SetRagdoll(bool enable)
    {
        m_isRagdoll = enable;
        m_animator.enabled = !enable;
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            collider.enabled = !enable;
        }
        foreach (Rigidbody rigidBody in RagdollRoot.GetComponentsInChildren<Rigidbody>())
        {
            rigidBody.isKinematic = !enable;
        }
        if (enable)
        {
            // The skinned mesh renderer must become a chid of the ragdoll root in order for the skinned mesh bounds
            // to follow the ragdoll, otherwise the ragdoll will disappear it the bounds are outside the camera view.
            RagdollRoot.parent = null;
            transform.parent = RagdollRoot;
        }
    }
}