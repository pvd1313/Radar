using System.Collections.Generic;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycle
    {
        public IReadOnlyDictionary<string, IProfiler> Profilers { get; }
        
        public bool TryInitializeSelf(GameCycleParameters input);
        
        UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) where TOwner : class;

        public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class;
    }
}