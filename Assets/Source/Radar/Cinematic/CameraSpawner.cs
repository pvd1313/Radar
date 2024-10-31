using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public class CameraSpawner : Controller, IGameCycleListener
    {
        private readonly EssentialPrefabLibrary _library;

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
            BScene.InstantiateMono
            (
                this,
                _library.camera,
                new float3(0, 0, -1),
                quaternion.identity
            );

            return UniTask.CompletedTask;
        }
    }
}