namespace Beast.Claw.GameLoop
{
    public struct BlockingContext
    {
        public void CancelEmptyMethod()
        {
            throw SubscriptionCancelledException.EmptyMethod;
        }
    }
}