using System;
using System.Collections.Generic;
using System.Text;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.UI;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI
{
    public partial class WindowManager : Actor, IWindowManager
    {
        public string WindowID => _currentWindow;
        
        public WindowOpener Open { get; private set; }
        
        [Required] private readonly EssentialPrefabLibrary _library;
        [Required] private readonly ICamera _camera;
        [Required] private readonly IScreen _screen;
        [Required] private readonly IInputManager _inputManager;

        private Dictionary<string, WindowData> _idToWindow;
        private List<Record> _debugHandleRecords;
        private string _currentWindow = UI.WindowID.NONE;
        
        private NoesisMonoRender _noesisRender;
        private CanvasView _canvas;
        private Grid _grid;
        private NoesisInputConsumer _inputConsumer;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _library.canvasXaml.Instantiate(out _canvas);
            _grid = _canvas.grid;
            
            _idToWindow = new Dictionary<string, WindowData>(64);

            if (Build.IS_DEVELOPMENT)
            {
                _debugHandleRecords = new List<Record>(256);
            }
            
            NoesisMonoRender render = BScene.InstantiateMono(this, _library.noesisRender);
            NoesisView view = render.Initialize(_camera.Camera, _canvas);

            _inputConsumer = new NoesisInputConsumer(view, _screen);
            _inputManager.RegisterConsumer(_inputConsumer, InputConsumerPriority.UI_NAVIGATION);

            Open = new WindowOpener(this);

            return UniTask.CompletedTask;
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
        
        public void Attach(UIElement uiElement)
        {
            uiElement.ValidateNotNullThrowException(nameof(uiElement));

            Panel.SetZIndex(uiElement, 0);
            
            _grid.Children.Add(uiElement);
        }

        public void Attach(UIElement uiElement, int index)
        {
            uiElement.ValidateNotNullThrowException(nameof(uiElement));

            Panel.SetZIndex(uiElement, index);
            
            _grid.Children.Add(uiElement);
        }
        
        public void Register<TData>(IWindow<TData> window, string windowID) where TData : struct
        {
            window.ValidateNotNullThrowException(nameof(window));
            windowID.ValidateNotEmptyOrNullThrowException(nameof(windowID));

            if (Build.IS_DEVELOPMENT)
            {
                if (_idToWindow.ContainsKey(windowID))
                {
                    throw new InvalidOperationException($"Window with '{windowID}' is already registered");
                }
            }

            Container<TData> container = new ();

            _idToWindow[windowID] = new WindowData
            {
                openWindow = () => window.Open(container.data),
                closeWindow = window.Close,
                dataContainer = container,
                dataType = typeof(TData),
                windowID = windowID
            };
        }
        
        private async UniTask _open<TData>(string windowID, TData data)
        {
            if (!_idToWindow.TryGetValue(windowID, out WindowData windowData))
            {
                throw new InvalidOperationException($"Window with id '{windowID}' was not found.");
            }

            if (Build.IS_DEVELOPMENT)
            {
                if (typeof(TData) != windowData.dataType)
                {
                    throw new InvalidOperationException($"Expected '{windowData.dataType.FullName}' data type to " +
                                                        $"open '{windowID}', but got '{{typeof(TData).FullName}}'");
                }
            }

            if (_currentWindow != UI.WindowID.NONE)
            {
                try
                {
                    await _idToWindow[_currentWindow].closeWindow();
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);    
                }
                
                _currentWindow = UI.WindowID.NONE;
            }

            Container<TData> container = (Container<TData>)windowData.dataContainer;
            container.data = data;
            
            try
            {
                await windowData.openWindow();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);    
            }

            _currentWindow = windowID;
            
            await UniTask.NextFrame();
        }

        protected override void _dispose(BlockingContext context)
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