namespace Beast.Claw.GameCycle
{
    public struct OperationInfo
    {
        public bool executedOncePerObject;
        public string id;

        public OperationInfo(string id, bool executedOncePerObject)
        {
            this.id = id;
            this.executedOncePerObject = executedOncePerObject;
        }
    }
}