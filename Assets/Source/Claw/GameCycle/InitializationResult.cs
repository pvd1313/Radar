using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public struct InitializationResult
    {
        public EInitializationStatus status;

        public static InitializationResult Success()
        {
            return new InitializationResult
            {
                status = EInitializationStatus.Success
            };
        }

        public static UniTask<InitializationResult> SuccessNotAsync()
        {
            return UniTask.FromResult(Success());
        }
    }
}