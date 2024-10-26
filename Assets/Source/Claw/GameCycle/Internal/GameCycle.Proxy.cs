using System.Collections.Generic;
using Beast.Claw.Profiling;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    partial class GameCycle
    {
        public class Proxy : IGameCycle
        {
            public IReadOnlyDictionary<string, IProfiler> Profilers => null;
        
            private GameCycle _gameCycle;
            // private Dictionary<string, IProfiler> _profilers;

            public void Inject(GameCycle gameCycle)
            {
                _gameCycle = gameCycle;
                // _profilers = new Dictionary<string, IProfiler>();
            }

            // public void InitializeProfilers()
            // {
            //     _profilers.Clear();
            //     
            //     foreach ((string name, Profiler profiler) in _gameCycle._profilers)
            //     {
            //         _profilers.Add(name, profiler);
            //     }
            // }

            public void Initialize(GameCycleInitialization input)
            {
                _gameCycle._initialize(input);
            }

            public void RegisterListeners()
            {
                _gameCycle._registerListeners();
            }

            public void Update<TOwner>(TOwner owner, string eventID) where TOwner : class
            {
                _gameCycle._update(owner, eventID);
            }

            public async UniTask<OperationResult> Execute<TOwner>(TOwner owner, string eventID) where TOwner : class
            {
                return await _gameCycle._execute(owner, eventID);
            }

            public void Dispose()
            {
                _gameCycle?.Dispose();
            }
        }
    }
}