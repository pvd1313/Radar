using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public struct InitializationResult
    {
        public EControllerStatus status;

        public static InitializationResult Success()
        {
            return new InitializationResult
            {
                status = EControllerStatus.Success
            };
        }

        public static UniTask<InitializationResult> SuccessNotAsync()
        {
            return UniTask.FromResult(Success());
        }
    }
}