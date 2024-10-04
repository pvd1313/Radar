using System;
using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Debug
{
    public class BeastDebugInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            yield return typeof(BeastDebugController);
        }
    }
}