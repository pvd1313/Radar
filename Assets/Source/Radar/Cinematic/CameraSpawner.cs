using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraSpawner : Controller, IGameCycleListener, ICamera
    {
        public Camera Camera => _camera;
        
        private readonly EssentialPrefabLibrary _library;

        private CameraView _view;
        private Camera _camera;

        public CameraSpawner(EssentialPrefabLibrary library)
        {
            _library = library;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
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