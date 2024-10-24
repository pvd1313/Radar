using System;
using Beast.Radar.InputHandling;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public class CameraMotionParameters
    {
        public EInputScheme[] schemes;
        public float zoomSpeed;
        public float motionSpeed;
    }
}