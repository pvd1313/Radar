using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycle
    {
        public IReadOnlyList<string> AllOperations { get; }
        
        public void Initialize(GameCycleInitialization input);

        public void RegisterListeners();
        
        public IProfiler TryGetProfiler(string operationName);

        UniTask<ActionResult> ExecuteConcurrent(IController owner, string eventID);

        public void ExecuteBlocking(IController owner, string eventID);

        public void Dispose();
    }
}