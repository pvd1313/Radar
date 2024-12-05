namespace Beast.Claw.Graphy.Modules
{
    public abstract class GraphyMonitor
    {
        public bool IsEnabled { get; private set; }

        public void SetEnable(bool enabled)
        {
            IsEnabled = enabled;
        }

        public virtual void Initialize(){}

        public abstract void Update();
    }
}