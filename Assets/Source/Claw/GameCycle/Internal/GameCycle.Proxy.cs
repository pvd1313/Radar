using System.Collections.Generic;
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

                if (!runner.Profiler.IsInitialized)
                {
                    return null;
                }

                return runner.Profiler.Profiler;
            }

            public void ExecuteBlocking<TOwner>(TOwner owner, string eventID) where TOwner : class
            {
                _gameCycle._update(owner, eventID);
            }

            public async UniTask<ActionResult> ExecuteConcurrent<TOwner>(TOwner owner, string eventID) where TOwner : class
            {
                return await _gameCycle._execute(owner, eventID);
            }

            public void Dispose()
            {
                _gameCycle?._dispose();
            }
        }
    }
}