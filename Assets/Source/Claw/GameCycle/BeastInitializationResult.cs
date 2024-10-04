using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public struct BeastInitializationResult
    {
        public EBeastInitializationStatus status;

        public static BeastInitializationResult Success()
        {
            return new BeastInitializationResult
            {
                status = EBeastInitializationStatus.Success
            };
        }

        public static UniTask<BeastInitializationResult> SuccessNotAsync()
        {
            return UniTask.FromResult(Success());
        }
    }
}