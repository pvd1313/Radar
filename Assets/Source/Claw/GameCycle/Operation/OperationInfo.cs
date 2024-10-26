namespace Beast.Claw.GameCycle
{
    public struct OperationInfo
    {
        public readonly bool executedOncePerObject;
        public readonly string id;

        public OperationInfo(string id, bool executedOncePerObject)
        {
            this.id = id;
            this.executedOncePerObject = executedOncePerObject;
        }
    }
}