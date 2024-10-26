using System;
using System.Collections.Generic;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public interface IGameCycle : IDisposable
    {
        public IReadOnlyDictionary<string, IProfiler> Profilers { get; }
        
        public void Initialize(GameCycleData input);

        public void RegisterListeners();
        
        UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) where TOwner : class;

        public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class;
    }
}