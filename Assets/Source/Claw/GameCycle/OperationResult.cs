using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public struct OperationResult
    {
        public EOperationStatus status;
        public string errorMessage;

        public bool IsSuccess()
        {
            return status == EOperationStatus.Success;
        }

        public bool IsFailed()
        {
            return status != EOperationStatus.Success;
        }

        public static OperationResult Success()
        {
            return new OperationResult
            {
                status = EOperationStatus.Success
            };
        }

        public static OperationResult Failed()
        {
            return new OperationResult
            {
                status = EOperationStatus.Failed
            };
        }

        public static UniTask<OperationResult> SuccessSyncronous()
        {
            return UniTask.FromResult(Success());
        }
    }
}