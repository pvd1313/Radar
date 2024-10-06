using System;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class BeastCameraConfig : BeastConfig
    {
        public BeastCameraView viewPrefab;
        public float minViewDistance;   
        public AnimationCurve zoomCurve;
        public BeastCameraMotionParameters[] motion;
        
        public BeastCameraMotionParameters GetMotionParameters(EBeastInputScheme scheme)
        {
            foreach (BeastCameraMotionParameters parameter in motion)
            {
                if (parameter.schemes.Contains(scheme))
                {
                    return parameter;
                }
            }

            throw new InvalidOperationException($"Failed to find motion parameters for scheme '{scheme}'");
        }
    }
}