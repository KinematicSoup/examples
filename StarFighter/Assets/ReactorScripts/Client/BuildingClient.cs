using System;
using System.Collections.Generic;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;

public class BuildingClient : ksEntityScript
{
    public Transform Explosion;

	public override void Initialize()
	{
        Entity.OnDestroy += Destroy;
	}
    public override void Detached()
    {
        Entity.OnDestroy -= Destroy;
    }

	public void Destroy(ksDestroyReason reason)
	{
        if (reason == ksDestroyReason.SERVER_DESTROY && !Room.GameObject.GetComponent<GameManagerClient>().GameOver)
        {
            Instantiate(Explosion, transform.position, transform.rotation);
        }
	}
}