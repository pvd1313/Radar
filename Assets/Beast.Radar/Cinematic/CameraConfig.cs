using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraConfig : BeastConfig
    {
        public CameraView viewPrefab;
        
        [Space]
        public float2 ellipseSize;
        public Vector3 startPosition;
        
        [Space]
        public float motionSpeed;
        public AnimationCurve inertiaDeceleration;
        
        [Space]
        public Float2MinMax zoomRange;
        public float zoomSpeed;

    }
}