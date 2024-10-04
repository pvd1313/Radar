using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Cinematic;
using Beast.Radar.InputControl;
using Beast.Radar.UI;
using Beast.Radar.World;

namespace Beast.Radar.Core
{
    public class CoreInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            // UI
            yield return typeof(BeastUIFactory);
            yield return typeof(BeastWindowManager);
            yield return typeof(BeastHUDWindow);
            yield return typeof(BeastPauseWindow);
            yield return typeof(InputBlocker);
            
            yield return typeof(PlayerInputProvider);
            yield return typeof(UIInputController);
            yield return typeof(CameraInputController);
            yield return typeof(CameraController);
            yield return typeof(TerrainBaker);
            yield return typeof(WorldMapRender);
        }
    }
}