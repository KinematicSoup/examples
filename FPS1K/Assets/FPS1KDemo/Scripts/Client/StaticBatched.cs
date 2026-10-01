using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Attach this to a static game object to make it statically batched will all other game objects created on the same
// frame that use the same material. This is used to batch the static entities that are randomly generated when the
// level is generated.
public class StaticBatched : MonoBehaviour
{
    private void Start()
    {
        if (StaticBatcher.Instance != null)
        {
            StaticBatcher.Instance.Add(gameObject);
        }
    }
}