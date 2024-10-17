using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Radar.InputControl;

namespace Beast.Radar.Cinematic
{
    public class CameraConfig : ScriptableConfig
    {
        public CameraTransformConfig transform;
        public CameraMotionParameters[] motion;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
        
        public CameraMotionParameters GetMotionParameters(EInputScheme scheme)
        {
            foreach (CameraMotionParameters parameter in motion)
            {
                if (parameter.schemes.Contains(scheme))
                {
                    return parameter;
                }
            }

            throw new InvalidOperationException($"Failed to find motion parameters for scheme '{scheme}'");
        }
    }
}