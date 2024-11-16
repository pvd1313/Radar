namespace Beast.Claw.GameLoop
{
    public struct BlockingContext
    {
        public void Abort(EActionAbortReason reason)
        {
            throw ActionAbortedException.FromReason(reason);
        }
    }
}