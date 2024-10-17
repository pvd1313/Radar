namespace Beast.Radar.UI
{
    public interface IPanel : IWindow
    {
        void Warmup();
        void Dispose();
    }
}