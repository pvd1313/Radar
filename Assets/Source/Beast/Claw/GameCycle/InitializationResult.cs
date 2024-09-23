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
    }
}