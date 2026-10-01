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
    
    public class seCharacter : ksProxyEntityScript
    {

        public Int32 MaxHealth;
        public Single GunOffsetY;
        public KSProxies.Scripts.sWeapon[] Weapons;
        public Int32 WeaponIndex;
        public Int32 Bullets;
        public Int32 MaxBullets;
        public Int32 Grenades;
        public Int32 MaxGrenades;
        public seCharacter() : base() 
        {
            MaxHealth = 100;
            GunOffsetY = 1.5f;
            Weapons = null;
            WeaponIndex = 0;
            Bullets = 40;
            MaxBullets = 100;
            Grenades = 20;
            MaxGrenades = 50;
        }

    }
}