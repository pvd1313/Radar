using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using UnityEngine;

namespace Beast.Radar.World
{
    public class WorldCameraConfig : BeastConfig
    {
        public Float2MinMax zoomRange;
        public float zoomSpeed;
        public float motionSpeed;
        public AnimationCurve zoomCurve;
    }
}