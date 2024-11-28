using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public class CameraTransformConfig : IConfig
    {
        public IsometricCameraRotation isometricRotationDeg;
        public float minViewDistance;
        public float plainModeViewDistance01;
        public bool enablePlanetMode;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}