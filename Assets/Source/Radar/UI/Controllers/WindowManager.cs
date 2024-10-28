using System;
using System.Collections.Generic;
using System.Text;
using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.UI
{
    public partial class WindowManager : Controller, IWindowManager, IGameCycleListener
    {
        public event Action<string> WindowChanged;
        public event Action<bool> MouseOverUI;

        public Transform CanvasTransform => _canvas.transform;
        public string CurrentWindow => _currentWindow;
        
        private readonly UIPrefabLibrary _library;
        private readonly IResourceManager _resources;
        private readonly IPrefabFactory _factory;
        
        private Dictionary<string, WindowData> _idToWindow;
        private List<Record> _debugHandleRecords;
        private CanvasView _canvas;
        
        private string _currentWindow = WindowID.None;
        
        public WindowManager(UIPrefabLibrary library, IResourceManager resources, IPrefabFactory factory /*, IInputSystem inputSystem*/)
        {
            _resources = resources;
            _factory = factory;
            _library = library;
            // _inputSystem = inputSystem;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
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
                throw new InvalidOperationException($"Windows with '{windowID}' is already registered");
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
        
        private async UniTask<ActionResult> _warmup(ActionContext context)
        {
            _idToWindow = new Dictionary<string, WindowData>(64);

            if (Build.IS_DEVELOPMENT)
            {
                _debugHandleRecords = new List<Record>(256);
            }

            CanvasView prefab = await _resources.LoadPrefab(this, _library.canvas);

            _canvas = await _factory.Instantiate(this, prefab);
            
            _canvas.MouseOverUI += _onMouseOverUI;

            return ActionResult.Success();
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

            if (_currentWindow != WindowID.None)
            {
                try
                {
                    await _idToWindow[_currentWindow].closeWindow();
                }
                catch (Exception e)
                {
                    BDebug.LogException(this, e, $"Failed to close window '{windowID}'");    
                }
                
                _currentWindow = WindowID.None;
            }

            DataContainer<TData> dataContainer = (DataContainer<TData>)windowData.dataContainer;
            dataContainer.data = data;
            
            try
            {
                await windowData.openWindow();
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, $"Failed to open window '{windowID}'");    
            }

            _currentWindow = windowID;
            
            WindowChanged?.Invoke(_currentWindow);
        }

        private void _onMouseOverUI(bool isMouseOverUI)
        {
            MouseOverUI?.Invoke(isMouseOverUI);       
        }
        
        private UniTask<ActionResult> _dispose(ActionContext arg)
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

            return ActionResult.SuccessSyncronous();
        }

        // private void _onOptionButtonsPressStarted(EInputOptionButton button)
        // {
        //     if (button == EInputOptionButton.Fourth)
        //     {
        //         if (_currentWindowID != WindowID.MainHUD)
        //         {
        //             Show(WindowID.MainHUD);
        //         }
        //     }
        //     else if(button == EInputOptionButton.Start)
        //     {
        //         Show(_currentWindow is not HUDWindow ? WindowID.MainHUD : WindowID.Pause);
        //     }
        // }
    }
}