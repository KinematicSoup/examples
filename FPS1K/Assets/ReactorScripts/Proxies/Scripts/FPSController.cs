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
    [CreateAssetMenu(menuName = ksMenuNames.REACTOR + "FPSController", order = ksMenuGroups.SCRIPT_ASSETS)]
    public class FPSController : ksPlayerControllerAsset
    {

        public Single MoveSpeed;
        public Single DashSpeed;
        public Single JumpSpeed;
        public Single MaxPitch;
        public Single MaxSlope;
        public Single SnapGroundDistance;
        public Single StepDistance;
        public Single MaxSlideAngle;
        public Single MinMoveDistance;
        public Single ImpulseMultiplier;
        public Single MaxPenetration;
        public FPSController() : base() 
        {
            MoveSpeed = 5f;
            DashSpeed = 8f;
            JumpSpeed = 6f;
            MaxPitch = 75f;
            MaxSlope = 1f;
            SnapGroundDistance = 0.05f;
            StepDistance = 0.25f;
            MaxSlideAngle = 80f;
            MinMoveDistance = 0.01f;
            ImpulseMultiplier = 0.5f;
            MaxPenetration = 0.25f;
            m_useInputPrediction = true;
        }

    }
}