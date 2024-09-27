using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Beast.Radar.InputControl;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraConfig : BeastConfig
    {
        public CameraView viewPrefab;
        [Space]
        public float2 ellipseSize;
        public float2 startPosition;
        public Float2MinMax zoomRange;   
        public AnimationCurve zoomCurve;
        [Space]
        public CameraParams[] cameraParams;
        
        public CameraParams GetParams(EInputScheme scheme)
        {
            return cameraParams.FirstOrDefault(p => p.schemes.Contains(scheme));
        }
    }
}