using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Cinematic
{
    public class CameraConfig : ScriptableConfig
    {
        public CameraTransformConfig transform;
        public float zoomSmoothSpeed;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return transform;
        }
    }
}