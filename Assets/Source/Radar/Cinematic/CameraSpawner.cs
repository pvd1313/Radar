using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraSpawner : Actor, ICamera
    {
        public Camera Camera => _camera;
        
        [Required] private readonly EssentialPrefabLibrary _library;

        private CameraView _view;
        private Camera _camera;

        protected override UniTask _warmup(ConcurrentContext context)
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