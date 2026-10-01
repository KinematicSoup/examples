/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KS.Reactor.Client.Unity;
using KS.Reactor;
using KS.Unity;

namespace KSProxies.Scripts
{
    
    public class srItemSpawner : ksProxyRoomScript
    {
#if UNITY_EDITOR
        public KSProxies.Structs.srItemSpawner.ItemInfo[] Items;
        public Single ItemsPerTile;
        public srItemSpawner() : base() 
        {
            Items = null;
            ItemsPerTile = 0.5f;
        }
#endif
    }
}