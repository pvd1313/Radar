using System;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public class CameraTransformConfig
    {
        public IsometricCameraRotation isometricRotationDeg;
        public float minViewDistance;
        public float plainModeViewDistance01;
        public bool enablePlanetMode;
    }
}