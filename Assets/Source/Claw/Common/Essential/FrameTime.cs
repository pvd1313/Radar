using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Common
{
    public class FrameTime : Actor, IFrameTime
    {
        public float Delta { get; private set; }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            Delta = 1f / 60f;
            
            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            Delta = Time.deltaTime;
        }
    }
}