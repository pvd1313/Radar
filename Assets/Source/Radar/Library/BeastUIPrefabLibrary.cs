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
        public BeastCanvasPrefab[] canvas;
        public BeastWindowPrefab[] window;

        private Dictionary<string, BeastCanvasView> _canvas;
        private Dictionary<string, BeastUIWindowView> _window;
        
        public UniTask<InitializationResult> Initialize()
        {
            _canvas = canvas.ToDictionary(c => c.id, c => c.prefab);
            _window = window.ToDictionary(w => w.id, w => w.prefab);

            return InitializationResult.SuccessNotAsync();
        }

        public BeastCanvasView GetCanvas(string prefabID)
        {
            if (_canvas.TryGetValue(prefabID, out BeastCanvasView prefab))
            {
                return prefab;
            }
            
            throw new InvalidOperationException($"Prefab '{prefabID}' was not found");
        }

        public BeastUIWindowView GetWindow(string prefabID)
        {
            if (_window.TryGetValue(prefabID, out BeastUIWindowView prefab))
            {
                return prefab;
            }
            
            throw new InvalidOperationException($"Prefab '{prefabID}' was not found");
        }
    }
}