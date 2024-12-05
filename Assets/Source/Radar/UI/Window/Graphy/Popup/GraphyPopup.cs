namespace Beast.Radar.UI
{
    public abstract class GraphyPopup
    {
        public abstract void Initialize();
        public abstract void RefreshState();
        public virtual void Update() {}
    }
}