
/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KS.Reactor.Client.Unity;
using KS.Reactor;
using KS.Unity;

namespace KSProxies.Structs.srItemSpawner
{
    
    [Serializable]
    public struct ItemInfo
    {
        public String Name;
        public Single Weight;
        public ItemInfo(String a0, Single a1) 
        {
            Name = a0;
            Weight = a1;
        }
    }
}
