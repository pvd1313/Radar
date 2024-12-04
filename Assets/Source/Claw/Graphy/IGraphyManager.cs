using Beast.Claw.Graphy.Modules;

namespace Beast.Claw.Graphy
{
    public interface IGraphyManager
    {
        public FpsMonitor FpsMonitor { get; }
        public FpsRender FpsRender { get; }
        public InformationMonitor InformationMonitor { get; }
        public AudioMonitor AudioMonitor { get; }
        public AudioRender AudioRender { get; }
    }
}