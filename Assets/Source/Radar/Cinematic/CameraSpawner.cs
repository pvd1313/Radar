using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Cinematic
{
    public class CameraSpawner : Controller, IGameCycleListener, ICamera
    {
        public Camera Camera => _camera;
        
        private readonly EssentialPrefabLibrary _library;

        private CameraView _view;
        private Camera _camera;
        private int _uiMask;

        public CameraSpawner(EssentialPrefabLibrary library)
        {
            _library = library;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        // TODO: #R-93 Not working.
        public bool IsPointInsideUI(float2 viewportPoint)
        {
            Profiler.BeginSample("CameraSpawner.IsPointInsideUI");
            
            Ray ray = _view.camera.ViewportPointToRay(viewportPoint.XY0());
            
            bool result = Physics.Raycast(ray, 9999f, _uiMask);
            
            Profiler.EndSample();

            return result;
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _uiMask = 1 << LayerMask.NameToLayer("UI");
            
            _view = BScene.InstantiateMono
            (
                this,
                _library.camera,
                new float3(0, 0, -1),
                quaternion.identity
            );

            _camera = _view.camera;

            return UniTask.CompletedTask;
        }
    }
}