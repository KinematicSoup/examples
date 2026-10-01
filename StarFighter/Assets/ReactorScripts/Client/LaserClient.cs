using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;

public class LaserClient : ksEntityScript
{
    [Tooltip("The explosion prefab instantaited upon the laser's destuction.")]
    public Transform ExplosionPrefab;

    [Tooltip("The prefab instantiated when the laser hits something")]
    public Transform PlinkSound;
    [Tooltip("The number of seconds to wait before enabling the trail renderer.")]
    public float TrailDelay = .075f;
    private TrailRenderer m_trail = null;

    public override void Initialize()
    {
        Entity.OnDestroy += Destroy;
        SetMaterials();
        m_trail = GetComponent<TrailRenderer>();
        m_trail.time = 0.08f;
        m_trail.enabled = false;
    }

    public override void Detached()
    {
        Entity.OnDestroy -= Destroy;
    }

    private void Destroy(ksDestroyReason reason)
    {
        if (reason == ksDestroyReason.SERVER_DESTROY)
        {
            Transform explosion = Instantiate(ExplosionPrefab, Entity.ServerTransform.Position, Entity.ServerTransform.Rotation);
            ParticleSystem.MainModule ps = explosion.GetComponent<ParticleSystem>().main;
            ps.startColor = GetColor();

            // disconnects the sound source if the firing sound is still playing to prevent
            // it from being immediatly destroyed and cutting the clip short
            Transform sounds = transform.Find("sound");
            if (sounds.GetComponent<LaserSound>().FireSource.isPlaying)
            {
                sounds.SetParent(null);
            }
        }
    }

    private void Update()
    {
        if (m_trail.enabled)
        {
            return;
        }
        if (TrailDelay <= 0)
        {
            m_trail.enabled = true;
            return;
        }
        TrailDelay -= Time.Delta;
    }

    /*
     * Sets the materials to use the laser owner's color.
     */
    private void SetMaterials()
    {
        Color color = GetColor();
        GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", color);
        transform.Find("laserLight").GetComponent<Light>().color = color;
    }

    private Color GetColor()
    {
        ksPlayer player = Room.GetPlayer(Properties[ID.PROP.LASER.OWNER]);
        if (player != null)
        {
            return player.GameObject.GetComponent<PlayerClient>().Color;
        }
        return Color.white;
    }
    
    [ksRPC(ID.RPC.PLINK)]
    private void PlayPlinkSound()
    {
        Transform t = Instantiate(PlinkSound, Camera.main.transform.position, Quaternion.identity) as Transform;
        t.SetParent(transform.Find("TempSounds"));
    }
}