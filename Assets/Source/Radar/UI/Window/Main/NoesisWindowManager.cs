using Beast.Claw;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.UI;
using Cysharp.Threading.Tasks;
using Noesis;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class NoesisWindowManager : Controller, INoesisWindowManager, IGameCycleListener
    {
        private readonly EssentialPrefabLibrary _library;
        private readonly ICamera _camera;

        private NoesisMonoRender _render;
        private CanvasView _canvas;
        private Grid _grid;

        public NoesisWindowManager(EssentialPrefabLibrary library, ICamera camera)
        {
            _library = library;
            _camera = camera;
        }

        public void Attach(UIElement uiElement)
        {
            uiElement.ValidateNotNullThrowException(nameof(uiElement));

            _grid.Children.Add(uiElement);
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            _library.canvasXaml.Instantiate(out _canvas);
            _grid = _canvas.grid;
            
            NoesisMonoRender render = BScene.InstantiateMono(
                this, 
                _library.noesisRender, 
                float3.zero, 
                quaternion.identity);

            render.Setup(_camera.Camera, _canvas);
            
            return UniTask.CompletedTask;
        }
    }
}