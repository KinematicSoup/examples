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
    [CreateAssetMenu(menuName = ksMenuNames.REACTOR + "FighterController", order = ksMenuGroups.SCRIPT_ASSETS)]
    public class FighterController : ksPlayerControllerAsset
    {

        public Single m_speedSmoothing;
        public Single m_rollSmoothing;
        public Single m_strafeSmoothing;
        public Single m_deaccelerationSmoothing;
        public Single m_maxForward;
        public Single m_maxReverse;
        public Single m_maxStrafe;
        public Single m_maxTurnSpeed;
        public Single m_maxRollSpeed;
        public FighterController() : base() 
        {
            m_speedSmoothing = 0.05f;
            m_rollSmoothing = 0.2f;
            m_strafeSmoothing = 0.05f;
            m_deaccelerationSmoothing = 0.01f;
            m_maxForward = 6f;
            m_maxReverse = 3f;
            m_maxStrafe = 4f;
            m_maxTurnSpeed = 90f;
            m_maxRollSpeed = 150f;
            m_useInputPrediction = true;
        }

    }
}