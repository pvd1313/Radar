using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.InputControl;
using Beast.Radar.UI;

namespace Beast.Radar.Core
{
    public class UIInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            yield return typeof(UIFactory);
            yield return typeof(WindowManager);
            yield return typeof(MainHUDWindow);
            yield return typeof(PauseWindow);
            yield return typeof(InputBlocker);
        }
    }
}