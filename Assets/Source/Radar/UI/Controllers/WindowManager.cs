using System;
using System.Collections.Generic;
using System.Text;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public partial class WindowManager : Controller, IWindowManager, IGameCycleListener
    {
        public event Action<string> WindowChanged;

        public Transform CanvasTransform => _canvas.transform;

        public string CurrentWindow => _currentWindow;
        
        private readonly EssentialPrefabLibrary _library;
        private readonly ICamera _camera;

        private Dictionary<string, WindowData> _idToWindow;
        private List<Record> _debugHandleRecords;
        private LegacyCanvasView _canvas;
        
        private string _currentWindow = WindowID.NONE;
        
        public WindowManager(EssentialPrefabLibrary library, ICamera camera)
        {
            _library = library;
            _camera = camera;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }
        
        public IWindowHandle<TData> GetHandle<TData>(IController owner, string windowID) where TData : struct
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            windowID.ValidateNotEmptyOrNullThrowException(nameof(windowID));

            if (Build.IS_DEVELOPMENT)
            {
                _debugHandleRecords.Add(new Record
                {
                    obtainedBy = owner,
                    dataType = typeof(TData),
                    windowID = windowID
                });
            }
            
            return new Handle<TData>(this, windowID);
        }
        
        public void Register<TData>(IWindow<TData> window, string windowID) where TData : struct
        {
            window.ValidateNotNullThrowException(nameof(window));
            windowID.ValidateNotEmptyOrNullThrowException(nameof(windowID));

            if (Build.IS_DEVELOPMENT && _idToWindow.ContainsKey(windowID))
            {
                throw new InvalidOperationException($"Window with '{windowID}' is already registered");
            }

            DataContainer<TData> dataContainer = new ();

            _idToWindow[windowID] = new WindowData
            {
                openWindow = () => window.Open(dataContainer.data),
                closeWindow = window.Close,
                dataContainer = dataContainer,
                dataType = typeof(TData),
                windowID = windowID
            };
        }
        
        private UniTask _warmup(ConcurrentContext context)
        {
            _idToWindow = new Dictionary<string, WindowData>(64);

            if (Build.IS_DEVELOPMENT)
            {
                _debugHandleRecords = new List<Record>(256);
            }

            _canvas = BScene.InstantiateMono(this, _library.canvas, float3.zero, quaternion.identity);

            return UniTask.CompletedTask;
        }
        
        private async UniTask _open<TData>(string windowID, TData data)
        {
            if (!_idToWindow.TryGetValue(windowID, out WindowData windowData))
            {
                throw new InvalidOperationException($"Window with id '{windowID}' was not found.");
            }

            if (Build.IS_DEVELOPMENT && typeof(TData) != windowData.dataType)
            {
                string message = $"Expected '{windowData.dataType.FullName}' data type to open '{windowID}', " +
                                 $"but got '{typeof(TData).FullName}'";
                throw new InvalidOperationException(message);
            }

            if (_currentWindow != WindowID.NONE)
            {
                try
                {
                    await _idToWindow[_currentWindow].closeWindow();
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);    
                }
                
                _currentWindow = WindowID.NONE;
            }

            DataContainer<TData> dataContainer = (DataContainer<TData>)windowData.dataContainer;
            dataContainer.data = data;
            
            try
            {
                await windowData.openWindow();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);    
            }

            _currentWindow = windowID;
            
            WindowChanged?.Invoke(_currentWindow);

            await UniTask.NextFrame();
        }
        
        private void _dispose(BlockingContext context)
        {
            if (Build.IS_DEVELOPMENT)
            {
                StringBuilder builder = new();

                builder.AppendLine("Found invalid handles:");
                bool foundInvalidHandles = false;

                foreach (Record record in _debugHandleRecords)
                {
                    if (!_idToWindow.TryGetValue(record.windowID, out WindowData windowData))
                    {
                        builder.Append(record.obtainedBy.Meta.name);
                        builder.Append(": Unknown windowID (");
                        builder.Append(record.windowID);
                        builder.AppendLine(")");

                        foundInvalidHandles = true;
                        continue;
                    }

                    if (windowData.dataType != record.dataType)
                    {
                        builder.Append(record.obtainedBy.Meta.name);
                        builder.Append(": Window data type mistmatch (");
                        builder.Append(windowData.windowID);
                        builder.Append(") Expected: (");
                        builder.Append(windowData.dataType.FullName);
                        builder.Append("), but got (");
                        builder.Append(record.dataType.FullName);
                        builder.AppendLine(")");

                        foundInvalidHandles = true;
                    }
                }

                if (foundInvalidHandles)
                {
                    BDebug.LogError(this, builder.ToString());
                }
            }
        }
    }
}