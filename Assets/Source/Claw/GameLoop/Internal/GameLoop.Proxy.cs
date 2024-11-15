using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    partial class GameLoop
    {
        public class Proxy : IGameLoop
        {
            public IReadOnlyList<string> AllOperations => _gameLoop._operationIds;

            private GameLoop _gameLoop;

            public void Inject(GameLoop gameLoop)
            {
                _gameLoop = gameLoop;
            }

            public void Initialize(GameLoopSetup input)
            {
                _gameLoop._initialize(input);
            }

            public void RegisterListeners()
            {
                _gameLoop._registerListeners();
            }

            public IProfiler TryGetProfiler(string operationName)
            {
                if (!_gameLoop._runners.TryGetValue(operationName, out Runner runner))
                {
                    return null;
                }

                return runner.Profiler;
            }

            public void ExecuteBlocking(IController owner, string eventID)
            {
                _gameLoop._executeBlocking(owner, eventID);
            }

            public async UniTask<EActionStatus> ExecuteConcurrent(IController owner, string eventID)
            {
                return await _gameLoop._executeConcurrent(owner, eventID);
            }

            public void Dispose()
            {
                _gameLoop?._dispose();
            }
        }
    }
}