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
    
    public class srLevelGenerator : ksProxyRoomScript
    {
#if UNITY_EDITOR
        [ksReadOnly]
        public String[] TilePrefabs;
        public Int32 GridSize;
        public Int32 Seed;
        public Boolean IncludeDynamicRigidbodies;
        public srLevelGenerator() : base() 
        {
            TilePrefabs = null;
            GridSize = 3;
            Seed = 0;
            IncludeDynamicRigidbodies = false;
        }
#endif
    }
}