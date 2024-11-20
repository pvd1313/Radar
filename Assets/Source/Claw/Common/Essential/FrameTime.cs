using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Common
{
    public class FrameTime : Actor, IFrameTime
    {
        public float DeltaTimeFloat { get; private set; }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            DeltaTimeFloat = 1f / 60f;
            
            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            DeltaTimeFloat = Time.deltaTime;
        }
    }
}