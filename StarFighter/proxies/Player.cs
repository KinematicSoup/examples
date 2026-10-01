/* This file was auto-generated. DO NOT MODIFY THIS FILE. */
using System;
using UnityEngine;
using KS.Reactor.Client.Unity.Adaptors;
using KS.Reactor;

namespace KSProxies
{
    public class Player : ksProxyPlayerScript
    {
        [ksProperty(5000)]
        public String m_playerName = "";
        [ksProperty(5002)]
        public Int32 m_teamNumber = -2147483648;
        [ksProperty(5001)]
        public String m_teamName = "";
        [ksProperty(5003)]
        public Single m_teamColorR = 0f;
        [ksProperty(5004)]
        public Single m_teamColorG = 0f;
        [ksProperty(5005)]
        public Single m_teamColorB = 0f;
        [ksProperty(5009)]
        public Int32 m_respawnTimeLeft = 0;
    }
}