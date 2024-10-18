using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.JobUtilities;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public class GizmosStateStorage : IGizmosStateStorage, IGameCycleListener
    {
        public int Version { get; private set; }
        public int HandleCount => _lastHandleId;

        private Dictionary<string, ushort> _handleToId;
        private ushort _lastHandleId;
        private NativeArray<int> _activityChunks;
        private NativeArray<Color> _colors;
        private List<string> _names;


        //TODO: https://risenbeast.youtrack.cloud/issue/R-59/Tech-Feature-High
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, "warmup", _warmup);
            handle.ListenOperation(this, "dispose", _dispose);
        }

        private UniTask<OperationResult> _warmup()
        {
            _handleToId = new Dictionary<string, ushort>();
            _lastHandleId = 0;
            _activityChunks = NativeMemory.CreatePermArray<int>(0);
            _colors = NativeMemory.CreatePermArray<Color>(0);
            _names = new List<string>();
            return OperationResult.SuccessSync();
        }

        public ushort GetOrCreateHandleId(string name, Color defaultColor)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(name)} is null or empty");
            }

            _handleToId ??= new Dictionary<string, ushort>();
            if (_handleToId.TryGetValue(name, out ushort handleId))
            {
                return handleId;
            }
            
            if (_lastHandleId >= ushort.MaxValue)
            {
                throw new OutOfMemoryException("Too many handles");
            }
                
            handleId = _lastHandleId++;
            _handleToId[name] = handleId;
            ArrayMaintenanceData rule = new ArrayMaintenanceData
            {
                copyOnResize = true,
                itemPerAllocation = 32,
                requiredCapacity = _lastHandleId
            };
            NativeMemory.MaintainPersistentArrayLength(ref _activityChunks, rule);
            NativeMemory.MaintainPersistentArrayLength(ref _colors, rule);
            bool enabled = PlayerPrefs.GetInt(name, 1) > 0;
            _setChunkActivityBit(handleId, enabled);
            _names.Add(name);
            _colors[handleId] = defaultColor;
            Version++;

            return handleId;
        }

        public bool GetHandleEnabled(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _getChunkActivityBit(handleId);
        }

        public void SetHandleEnabled(ushort handleId, bool enabled)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            if (_getChunkActivityBit(handleId) == enabled)
            {
                return;
            }

            PlayerPrefs.SetInt(_names[handleId], enabled ? 1 : 0);
            _setChunkActivityBit(handleId, enabled);
            Version++;
        }

        public Color GetHandleColor(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _colors[handleId];
        }

        public void SetHandleColor(ushort handleId, Color color)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            if (_colors[handleId] == color)
            {
                return;
            }

            _colors[handleId] = color;
            Version++;
        }

        public string GetHandleName(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _names[handleId];
        }

        private void _setChunkActivityBit(ushort handleId, bool active)
        {
            if (active)
            {
                _activityChunks[handleId / 32] |= 1 << (handleId % 32);
            }
            else
            {
                _activityChunks[handleId / 32] &= ~(1 << handleId % 32);
            }
        }

        private bool _getChunkActivityBit(ushort handleId)
        {
            return (_activityChunks[handleId / 32] & (1 << handleId % 32)) > 0;
        }

        private UniTask<OperationResult> _dispose()
        {
            _activityChunks.Dispose();
            _colors.Dispose();

            BDebug.LogInfo(this, "dispose");
            return OperationResult.SuccessSync();
        }
    }
}