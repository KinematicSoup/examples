/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using UnityEngine;
using KS.Reactor.Client.Unity.Adaptors;
using KS.Reactor;

namespace KSProxies
{
    public class Turret : ksProxyEntityScript
    {
        [ksProperty(2000)]
        public Int32 m_playerID = -2147483648;
        [ksProperty(2001)]
        public String m_playerName = "";
        [ksProperty(2002)]
        public Single m_energy = 0f;
        [ksProperty(2003)]
        public Boolean m_energyFrozen = false;
        [ksProperty(2006, Smoothing = ksPropertyAttribute.Types.Smoothing.LINEAR, Min = 0f, Max = 1f)]
        public Single m_laserReloadProgress = 0f;
        [ksProperty(2020, Smoothing = ksPropertyAttribute.Types.Smoothing.SPHERICAL, Min = 0f, Max = 360f)]
        public Single m_rotation = 0f;
        [ksProperty(2021, Smoothing = ksPropertyAttribute.Types.Smoothing.SPHERICAL, Min = 0f, Max = 360f)]
        public Single m_elevation = 0f;
    }
}