using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    [Serializable]
    public class UIPrefabLibrary : IConfig, IGameCycleListener
    {
        public CanvasView canvas;
        public UIPrefab[] window;

        private Dictionary<string, UIView> _window;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
        }
        
        public UniTask<OperationResult> _warmup(OperationContext context)
        {
            _window = window.ToDictionary(w => w.id, w => w.prefab);

            return OperationResult.SuccessSync();
        }

        public UIView GetWindow(string prefabID)
        {
            if (_window.TryGetValue(prefabID, out UIView prefab))
            {
                return prefab;
            }
            
            throw new InvalidOperationException($"Prefab '{prefabID}' was not found");
        }
    }
}