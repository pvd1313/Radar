using Beast.Claw.GameLoop;
using Beast.Claw.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugController : Actor, IDebugController
    {
        private readonly DebugPrefabLibrary _library;
        private readonly DebugPreference _preference;
        private readonly IResourceManager _resources;

        private GameObject _logConsole;
        private GameObject _fpsCounter;

        public DebugController(
            DebugPrefabLibrary library, 
            DebugPreference preference, 
            IResourceManager resources)
        {
            _library = library;
            _preference = preference;
            _resources = resources;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            UniTask.Create(async () =>
            {
                _logConsole = await _resources.InstantiateGameObject(this, _library.logConsole);
                _logConsole.SetActive(_preference.showLogConsole);

                _fpsCounter = await _resources.InstantiateGameObject(this, _library.fpsCounter);
                _fpsCounter.SetActive(_preference.showFPSCounter);
            });

            return UniTask.CompletedTask;
        }

        public void SetElementState(EDebugFeature id, bool isActive)
        {
            switch (id)
            {
                case EDebugFeature.FpsCounter:
                    
                    _preference.showFPSCounter = isActive;
                    
                    if (_fpsCounter != null)
                    {
                        _fpsCounter.SetActive(isActive);
                    }
                    
                    break;
                
                case EDebugFeature.LogConsole:
                    
                    _preference.showLogConsole = isActive;
                    
                    if (_logConsole != null)
                    {
                        _logConsole.SetActive(isActive);
                    }
                    
                    break;
            }
        }
        
        public bool GetElementState(EDebugFeature feature)
        {
            switch (feature)
            {
                case EDebugFeature.FpsCounter:
                    return _preference.showFPSCounter;
                
                case EDebugFeature.LogConsole:
                    return _preference.showLogConsole;
            }

            return false;
        }
    }
}