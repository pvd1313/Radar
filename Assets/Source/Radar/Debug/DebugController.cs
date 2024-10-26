using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugController : IDebugController, IGameCycleListener
    {
        private readonly DebugConfig _config;
        private readonly DebugPreferences _preferences;
        private readonly Dictionary<string, GameObject> _debugElements = new();

        public DebugController(DebugConfig config, DebugPreferences preferences)
        {
            _config = config;
            _preferences = preferences;
        }
    
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
        }

        private UniTask<OperationResult> _warmup(OperationContext context)
        {
            SetElementState(DebugElementID.LogConsoleID, _preferences.isShowLog);
            SetElementState(DebugElementID.FpsCounterID, _preferences.isShowFps);
            
            return OperationResult.SuccessSyncronous();
        }

        public void SetElementState(string elementID, bool isActive)
        {
            switch (elementID)
            {
                case DebugElementID.FpsCounterID:
                    _setElementsState(elementID, isActive, _config.fpsCounterPrefab);
                    _preferences.isShowFps = isActive;
                    break;
                
                case DebugElementID.LogConsoleID:
                    _setElementsState(elementID, isActive, _config.logConsolePrefab);
                    _preferences.isShowLog = isActive;
                    break;
            }
        }
        
        public bool GetElementState(string elementID)
        {
            return _debugElements.ContainsKey(elementID) && _debugElements[elementID].activeSelf;
        }

        private void _setElementsState(string elementID, bool isActive, GameObject prefab)
        {
            if (_debugElements.TryGetValue(elementID, out GameObject element))
            {
                element.SetActive(isActive);
            }
            else if(isActive)
            {
                _debugElements.Add(elementID, 
                    BScene.InstantiateGameObject(this, prefab, float3.zero, quaternion.identity));
            }
            else
            {
                return;
            }
            _preferences.isShowLog = isActive;
        }

    }
}