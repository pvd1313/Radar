using System;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraConfig : Config
    {
        public CameraView viewPrefab;
        public float minViewDistance;   
        public AnimationCurve zoomCurve;
        public CameraMotionParameters[] motion;
        
        public CameraMotionParameters GetMotionParameters(EInputScheme scheme)
        {
            foreach (CameraMotionParameters parameter in motion)
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