using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugController : Controller, IDebugController, IGameCycleListener
    {
        private readonly CorePrefabLibrary _library;
        private readonly DebugPreferences _preferences;
        private readonly IResourceManager _resources;

        private GameObject _logConsole;
        private GameObject _fpsCounter;

        public DebugController(CorePrefabLibrary library, DebugPreferences preferences, IResourceManager resources)
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
                DebugPrefabLibrary debug = await _resources.LoadAsset(this, _library.debug);

                GameObject logConsolePrefab = await _resources.LoadGameObject(this, debug.logConsole);
                _logConsole = await BFactory.InstantiateGameObject(this, logConsolePrefab);
                _logConsole.SetActive(_preferences.showLogConsole);

                GameObject fpsCounterPrefab = await _resources.LoadGameObject(this, debug.fpsCounter);
                _fpsCounter = await BFactory.InstantiateGameObject(this, fpsCounterPrefab);
                _fpsCounter.SetActive(_preferences.showFPSCounter);
            });

            return UniTask.CompletedTask;
        }

        public void SetElementState(DebugElementID id, bool isActive)
        {
            switch (id)
            {
                case DebugElementID.FpsCounterID:
                    
                    _preferences.showFPSCounter = isActive;
                    
                    if (_fpsCounter != null)
                    {
                        _fpsCounter.SetActive(isActive);
                    }
                    
                    break;
                
                case DebugElementID.LogConsoleID:
                    
                    _preferences.showLogConsole = isActive;
                    
                    if (_logConsole != null)
                    {
                        _logConsole.SetActive(isActive);
                    }
                    
                    break;
            }
        }
        
        public bool GetElementState(DebugElementID elementID)
        {
            switch (elementID)
            {
                case DebugElementID.FpsCounterID:
                    return _preferences.showFPSCounter;
                
                case DebugElementID.LogConsoleID:
                    return _preferences.showLogConsole;
            }

            return false;
        }
    }
}