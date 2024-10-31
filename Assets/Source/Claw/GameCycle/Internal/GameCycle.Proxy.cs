using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    partial class GameCycle
    {
        public class Proxy : IGameCycle
        {
            public IReadOnlyList<string> AllOperations => _gameCycle._operationIds;

            private GameCycle _gameCycle;

            public void Inject(GameCycle gameCycle)
            {
                _gameCycle = gameCycle;
            }

            public void Initialize(GameCycleInitialization input)
            {
                _gameCycle._initialize(input);
            }

            public void RegisterListeners()
            {
                _gameCycle._registerListeners();
            }

            public IProfiler TryGetProfiler(string operationName)
            {
                if (!_gameCycle._runners.TryGetValue(operationName, out Runner runner))
                {
                    return null;
                }

                return runner.Profiler;
            }

            public void ExecuteBlocking(IController owner, string eventID)
            {
                _gameCycle._executeBlocking(owner, eventID);
            }

            public async UniTask<ActionResult> ExecuteConcurrent(IController owner, string eventID)
            {
                return await _gameCycle._executeConcurrent(owner, eventID);
            }

            public void Dispose()
            {
                _gameCycle?._dispose();
            }
        }
    }
}