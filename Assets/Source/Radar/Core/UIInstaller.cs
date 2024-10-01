using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using Beast.Radar.UI.Canvas;
using Beast.Radar.UI.Factory;
using Beast.Radar.UI.Panels.Hud;
using Beast.Radar.UI.Panels.Pause;

namespace Beast.Radar.Core
{
    public class UIInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            yield return typeof(UIFactory);
            yield return typeof(CanvasController);
            yield return typeof(HudController);
            yield return typeof(PauseController);
            yield return typeof(InputBlockerOnUIHover);
        }
    }
}