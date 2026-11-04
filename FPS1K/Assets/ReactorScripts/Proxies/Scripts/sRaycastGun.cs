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
    [CreateAssetMenu(menuName = ksMenuNames.REACTOR + "sRaycastGun", order = ksMenuGroups.SCRIPT_ASSETS)]
    public class sRaycastGun : KSProxies.Scripts.sAmmoGun
    {

        public Int32 Damage;
        public Int32 HeadshotDamage;
        public Single Impulse;
        public Single Range;
        public Single PredictedPositionTolerance;
        public sRaycastGun() : base() 
        {
            Damage = 5;
            HeadshotDamage = 100;
            Impulse = 5f;
            Range = 200f;
            PredictedPositionTolerance = 1f;
            AmmoType = (KSProxies.Enums.AmmoTypes)0;
            Cooldown = 0.25f;
        }

    }
}