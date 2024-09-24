using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.World;

namespace Beast.Radar.Core
{
    public class CoreInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            yield return typeof(CameraController);
            yield return typeof(WorldMapBaker);
            yield return typeof(WorldMapRender);
        }
    }
}