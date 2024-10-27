using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameCycle
{
    public struct ActionResult
    {
        public EActionStatus status;
        public string errorMessage;

        public bool IsSuccess()
        {
            return status == EActionStatus.Success;
        }

        public bool IsFailed()
        {
            return status != EActionStatus.Success;
        }

        public static ActionResult Success()
        {
            return new ActionResult
            {
                status = EActionStatus.Success
            };
        }

        public static ActionResult Failed()
        {
            return new ActionResult
            {
                status = EActionStatus.Failed
            };
        }

        public static UniTask<ActionResult> SuccessSyncronous()
        {
            return UniTask.FromResult(Success());
        }
    }
}