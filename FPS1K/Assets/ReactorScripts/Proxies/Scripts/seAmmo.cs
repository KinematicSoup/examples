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
    
    public class seAmmo : KSProxies.Scripts.sePickUp
    {
#if UNITY_EDITOR
        public KSProxies.Enums.AmmoTypes AmmoType;
        public Int32 Amount;
        public seAmmo() : base() 
        {
            AmmoType = (KSProxies.Enums.AmmoTypes)0;
            Amount = 20;
        }
#endif
    }
}