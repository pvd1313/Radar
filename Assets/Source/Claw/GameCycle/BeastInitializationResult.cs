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

        public static BeastInitializationResult Failed()
        {
            return new BeastInitializationResult
            {
                status = EBeastInitializationStatus.Failed
            };
        }

        public static UniTask<BeastInitializationResult> SuccessSync()
        {
            return UniTask.FromResult(Success());
        }
    }
}