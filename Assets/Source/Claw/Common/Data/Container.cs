namespace Beast.Claw.Common
{
    public class Container<TValue>
    {
        public TValue data;

        public Container<TValue> SetData(TValue value)
        {
            data = value;

            return this;
        }
    }
}