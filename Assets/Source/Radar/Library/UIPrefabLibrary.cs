using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;

namespace Beast.Radar
{
    [Serializable]
    public class UIPrefabLibrary
    {
        public CanvasView canvas;
        public UIPrefab[] window;

        private Dictionary<string, UIView> _window;
        
        public UniTask<OperationResult> Initialize()
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