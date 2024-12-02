using Tayx.Graphy.Fps;

namespace Beast.Claw.Graphy
{
    public interface IGraphyManager
    {
        public GraphyFpsMonitor FpsMonitor { get; }
        public GraphyFpsRender FpsRender { get; }
    }
}