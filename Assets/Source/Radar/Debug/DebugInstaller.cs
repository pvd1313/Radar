using System;
using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Debug
{
    public class DebugInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            yield return typeof(DebugController);
        }
    }
}