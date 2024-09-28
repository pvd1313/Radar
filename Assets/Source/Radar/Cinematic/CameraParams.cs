using System;
using Beast.Radar.InputControl;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public class CameraParams //todo switch to struct 
    {
        public EInputScheme[] schemes;
        public float zoomSpeed;
        public float motionSpeed;
    }
}