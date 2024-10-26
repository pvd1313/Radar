namespace Beast.Claw.GameCycle
{
    public struct OperationInfo : IValidatable
    {
        public EOperationFlowType flow;
        public EOperationFrequency frequency;
        public EOperationOrder order;
        public string id;

        public string Validate()
        {
            if (string.IsNullOrEmpty(id))
            {
                return "id is empty";
            }

            if (flow == EOperationFlowType.Invalid)
            {
                return "Invalid type";
            }

            if (frequency == EOperationFrequency.Invalid)
            {
                return "Invalid frequency";
            }

            if (order == EOperationOrder.Invalid)
            {
                return "Invalid order";
            }

            return null;
        }
    }
}