using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Cinematic;
using Beast.Radar.InputControl;
using Beast.Radar.World;

namespace Beast.Radar.Core
{
    public class CoreInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            yield return typeof(PlayerInputProvider);
            yield return typeof(InputController);
            yield return typeof(CameraController);
            yield return typeof(WorldMapBaker);
            yield return typeof(WorldMapRender);
        }
    }
}