namespace Beast.Claw.GameCycle
{
    public struct ActionInfo : IValidatable
    {
        public EActionFlowType flow;
        public EActionFrequency frequency;
        public EActionOrder order;
        public int bufferCapacity;
        public string id;

        public string Validate()
        {
            if (string.IsNullOrEmpty(id))
            {
                return "id is empty";
            }

            if (bufferCapacity <= 0)
            {
                return "Invalid buffer capacity: " + bufferCapacity;
            }

            if (flow == EActionFlowType.Invalid)
            {
                return "Invalid type";
            }

            if (frequency == EActionFrequency.Invalid)
            {
                return "Invalid frequency";
            }

            if (order == EActionOrder.Invalid)
            {
                return "Invalid order";
            }

            return null;
        }
    }
}