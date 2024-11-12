using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;
using Noesis;
using Unity.Mathematics;

namespace Beast.Radar.UI.Xaml
{
    public class NoesisWindowManager : Controller, IGameCycleListener
    {
        private readonly EssentialPrefabLibrary _library;
        private readonly ICamera _camera;

        private NoesisView _view;
        private FrameworkElement _root;

        public NoesisWindowManager(EssentialPrefabLibrary library, ICamera camera)
        {
            _library = library;
            _camera = camera;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        private UniTask _warmup(ConcurrentContext arg)
        {
            NoesisView view = BScene.InstantiateMono(
                this, 
                _library.noesisView, 
                float3.zero, 
                quaternion.identity);

            _root = view.Initialize(_camera.Camera, _library.canvasXaml);
            
            return UniTask.CompletedTask;
        }
    }
}