using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraConfig : BeastConfig
    {
        public CameraView viewPrefab;
        public PlayerInputView PlayerInputView;
        
        [Space]
        public float2 ellipseSize;
        public float2 startPosition;
        
        [Space]
        public Float2MinMax zoomRange;
        public float zoomSpeed;
        public AnimationCurve zoomCurve;
    }
}