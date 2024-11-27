using System.Linq;
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
    public class NoesisWindowManager : Actor, INoesisWindowManager
    {
        private readonly EssentialPrefabLibrary _library;
        private readonly ICamera _camera;
        private readonly IScreen _screen;
        private readonly IInputManager _inputManager;

        private NoesisMonoRender _noesisRender;
        private CanvasView _canvas;
        private Grid _grid;
        private NoesisInputConsumer _inputConsumer;

        public NoesisWindowManager(
            EssentialPrefabLibrary library, 
            ICamera camera, 
            IScreen screen,
            IInputManager inputManager)
        {
            _library = library;
            _camera = camera;
            _screen = screen;
            _inputManager = inputManager;
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            _library.canvasXaml.Instantiate(out _canvas);
            _grid = _canvas.grid;
            
            NoesisMonoRender render = BScene.InstantiateMono(this, _library.noesisRender);

            NoesisView view = render.Initialize(_camera.Camera, _canvas);

            _inputConsumer = new NoesisInputConsumer(view, _screen);
            _inputManager.RegisterConsumer(_inputConsumer, InputConsumerPriority.UI_NAVIGATION);
            
            return UniTask.CompletedTask;
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
    }
}