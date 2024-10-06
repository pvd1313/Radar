using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    [Serializable]
    public class BeastUIPrefabLibrary
    {
        public BeastCanvasView canvas;
        public BeastUIPrefab[] window;

        private Dictionary<string, BeastUIView> _window;
        
        public UniTask<BeastInitializationResult> Initialize()
        {
            _window = window.ToDictionary(w => w.id, w => w.prefab);

            return BeastInitializationResult.SuccessSync();
        }

        public BeastUIView GetWindow(string prefabID)
        {
            if (_window.TryGetValue(prefabID, out BeastUIView prefab))
            {
                return prefab;
            }
            
            throw new InvalidOperationException($"Prefab '{prefabID}' was not found");
        }
    }
}