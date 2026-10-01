using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;

public class MissileClient : ksEntityScript
{
    [Tooltip("The explosion prefab instantiated upon the missiles's destuction.")]
    public Transform ExplosionPrefab;

    public override void Initialize()
    {
        Entity.OnDestroy += Destroy;

        Color trailColor = GetColor();
        trailColor.a = 0.3f;
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
	}

    public override void Detached()
    {
        Entity.OnDestroy -= Destroy;
    }

    private void Destroy(ksDestroyReason reason)
    {
        if (reason == ksDestroyReason.SERVER_DESTROY)
        {
            Instantiate(ExplosionPrefab, Entity.Transform.Position, Entity.Transform.Rotation);
        }
    }

    private Color GetColor()
    {
        ksPlayer player = Room.GetPlayer(Properties[ID.PROP.MISSILE.OWNER]);
        if (player != null)
        {
            return player.GameObject.GetComponent<PlayerClient>().Color;
        }
        return Color.white;
    }
}