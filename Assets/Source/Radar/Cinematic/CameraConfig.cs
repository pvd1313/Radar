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
        public float2 startPosition;
        public BeastFloatMinMax zoomRange;   
        public AnimationCurve zoomCurve;
        public CameraParams[] cameraParams;
        
        public CameraParams GetParams(EBeastInputScheme scheme)
        {
            return cameraParams.FirstOrDefault(p => p.schemes.Contains(scheme));
        }
    }
}