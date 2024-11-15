using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Input
{
    public interface IInputDevice : IController
    {
        public string FriendlyName { get; }
        
        public UniTask Warmup(ConcurrentContext context);

        public void Deactivate();

        public PackedAxis GetActiveAxis();

        public void Tick(BlockingContext context);
        
        public void Dispose(BlockingContext context);
        void Activate();
    }
}