using System;
using Beast.Radar.InputControl;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public class BeastCameraMotionParameters
    {
        public EBeastInputScheme[] schemes;
        public float zoomSpeed;
        public float motionSpeed;
    }
}