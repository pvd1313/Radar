using System;
using Beast.Radar.InputControl;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public struct CameraParams
    {
        public EInputScheme[] inputSchemes;
        [Space]
        public AnimationCurve zoomCurve;
        public float zoomSpeed;
        public float motionSpeed;
    }
}