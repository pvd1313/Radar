using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public class CameraSpawner : IGameCycleListener
    {
        private readonly PrefabLibrary _prefabs;

        public CameraSpawner(PrefabLibrary prefabs)
        {
            _prefabs = prefabs;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
        }

        private UniTask<OperationResult> _warmup(OperationContext context)
        {
            BScene.InstantiateMono
            (
                this,
                _prefabs.cameraPrefab,
                new float3(0, 0, -1),
                quaternion.identity
            );

            return OperationResult.SuccessSyncronous();
        }
    }
}