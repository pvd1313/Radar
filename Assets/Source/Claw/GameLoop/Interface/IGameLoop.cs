using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public interface IGameLoop
    {
        public IReadOnlyList<string> AllOperations { get; }
        
        public void Initialize(GameLoopSetup input);

        public void RegisterListeners();
        
        public IProfiler TryGetProfiler(string operationName);

        UniTask<EActionStatus> ExecuteConcurrent(IController owner, string eventID);

        public void ExecuteBlocking(IController owner, string eventID);

        public void Dispose();
    }
}