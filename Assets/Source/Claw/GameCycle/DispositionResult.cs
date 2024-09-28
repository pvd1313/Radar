using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public struct DispositionResult
    {
        public EControllerStatus status;

        public static DispositionResult Success()
        {
            return new DispositionResult
            {
                status = EControllerStatus.Success
            };
        }

        public static UniTask<DispositionResult> SuccessNotAsync()
        {
            return UniTask.FromResult(Success());
        }
    }
}