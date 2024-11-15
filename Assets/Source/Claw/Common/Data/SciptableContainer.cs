namespace Beast.Claw.Common
{
    public abstract class ScriptableContainer<TData> : Scriptable where TData : class
    {
        public TData data;
    }
}