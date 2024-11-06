using System;
using System.Collections.Generic;
using Beast.Claw.Jobs;
using Unity.Collections;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public static class GizmosStateStorage 
    {
        public static int HandleCount => _lastHandleId;
        public static bool Initialized { get; private set; }
        private static int Version { get; set; }

        private static Dictionary<string, ushort> _handleToId = new();
        private static ushort _lastHandleId;
        private static NativeArray<int> _activityChunks;
        private static NativeArray<Color> _colors;
        private static  List<string> _names = new();
        
        public static void Initialize()
        {
            _handleToId = new Dictionary<string, ushort>();
            _lastHandleId = 0;
            _activityChunks = Native.CreatePermArray<int>(0);
            _colors = Native.CreatePermArray<Color>(0);
            _names = new List<string>();
            
            Initialized = true;
        }
        
        public static ushort GetOrCreateHandleId(string name, Color defaultColor)
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
            Native.MaintainPersistentArrayLength(ref _activityChunks, rule);
            Native.MaintainPersistentArrayLength(ref _colors, rule);
            bool enabled = PlayerPrefs.GetInt(name, 1) > 0;
            _setChunkActivityBit(handleId, enabled);
            _names.Add(name);
            _colors[handleId] = defaultColor;
            Version++;

            return handleId;
        }

        public static bool GetHandleEnabled(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _getChunkActivityBit(handleId);
        }

        public static void SetHandleEnabled(ushort handleId, bool enabled)
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

        public static Color GetHandleColor(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _colors[handleId];
        }

        public static void SetHandleColor(ushort handleId, Color color)
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

        public static string GetHandleName(ushort handleId)
        {
            if (handleId >= _lastHandleId)
            {
                throw new ArgumentException($"{nameof(handleId)} is not created");
            }

            return _names[handleId];
        }

        public static void Dispose()
        {
            Initialized = false;
            if (_activityChunks.IsCreated)
            {
                _activityChunks.Dispose();
            }
            if (_colors.IsCreated)
            {
                _colors.Dispose();
            }
        }
        
        private static void _setChunkActivityBit(ushort handleId, bool active)
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

        private static bool _getChunkActivityBit(ushort handleId)
        {
            return (_activityChunks[handleId / 32] & (1 << handleId % 32)) > 0;
        }
    }
}