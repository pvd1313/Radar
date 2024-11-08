using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugController : Controller, IDebugController, IGameCycleListener
    {
        private readonly DebugPrefabLibrary _library;
        private readonly DebugPreferences _preferences;
        private readonly IResourceManager _resources;

        private GameObject _logConsole;
        private GameObject _fpsCounter;

        public DebugController(DebugPrefabLibrary library, DebugPreferences preferences, IResourceManager resources)
        {
            _library = library;
            _preferences = preferences;
            _resources = resources;
        }
    
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            UniTask.Create(async () =>
            {
                GameObject logConsolePrefab = await _resources.LoadGameObject(this, _library.logConsole);
                _logConsole = await BFactory.InstantiateGameObject(this, logConsolePrefab);
                _logConsole.SetActive(_preferences.showLogConsole);

                GameObject fpsCounterPrefab = await _resources.LoadGameObject(this, _library.fpsCounter);
                _fpsCounter = await BFactory.InstantiateGameObject(this, fpsCounterPrefab);
                _fpsCounter.SetActive(_preferences.showFPSCounter);
            });

            return UniTask.CompletedTask;
        }

        public void SetElementState(EDebugFeature id, bool isActive)
        {
            switch (id)
            {
                case EDebugFeature.FpsCounter:
                    
                    _preferences.showFPSCounter = isActive;
                    
                    if (_fpsCounter != null)
                    {
                        _fpsCounter.SetActive(isActive);
                    }
                    
                    break;
                
                case EDebugFeature.LogConsole:
                    
                    _preferences.showLogConsole = isActive;
                    
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
                    return _preferences.showFPSCounter;
                
                case EDebugFeature.LogConsole:
                    return _preferences.showLogConsole;
            }

            return false;
        }
    }
}