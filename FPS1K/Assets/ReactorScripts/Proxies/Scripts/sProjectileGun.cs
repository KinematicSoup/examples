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
    [CreateAssetMenu(menuName = ksMenuNames.REACTOR + "sProjectileGun", order = ksMenuGroups.SCRIPT_ASSETS)]
    public class sProjectileGun : KSProxies.Scripts.sAmmoGun
    {

        public String BulletPrefab;
        public Single Speed;
        public Single AdditiveSpeed;
        public Boolean AddStrafeSpeed;
        public Single GravityMultiplier;
        public Int32 Damage;
        public Single Impulse;
        public Single ShootOffset;
        public sProjectileGun() : base() 
        {
            BulletPrefab = "Bullet";
            Speed = 25f;
            AdditiveSpeed = 1f;
            AddStrafeSpeed = false;
            GravityMultiplier = 0f;
            Damage = 30;
            Impulse = 10f;
            ShootOffset = 0.25f;
            AmmoType = (KSProxies.Enums.AmmoTypes)0;
            Cooldown = 0.25f;
        }

    }
}