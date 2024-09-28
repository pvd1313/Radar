using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Coroutines;
using Beast.Claw.Debugging;

namespace Beast.Claw.GameCycle
{
    public class GameCycleInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> CreateContract()
        {
            return new[]
            {
                typeof(GameCycleLogic),
                typeof(CoroutineLogic),
                typeof(GizmosLogic)
            };
        }
    }
}