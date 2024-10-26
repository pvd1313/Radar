using System;
using System.Collections.Generic;
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
        
        UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) where TOwner : class;

        public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class;

        public void Dispose();
    }
}