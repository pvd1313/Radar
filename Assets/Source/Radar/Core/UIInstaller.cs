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
            yield return typeof(PanelDispatcher);
            yield return typeof(MainHUDController);
            yield return typeof(PauseController);
            yield return typeof(InputBlockerOnUIHover);
        }
    }
}