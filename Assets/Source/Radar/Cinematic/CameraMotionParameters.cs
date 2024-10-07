using System;
using Beast.Radar.InputControl;

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