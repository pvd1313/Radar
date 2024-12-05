using Beast.Claw.Graphy.Modules;
using Beast.Claw.Graphy.Modules.Ram;

namespace Beast.Claw.Graphy
{
    public interface IGraphyManager
    {
        public FpsMonitor FpsMonitor { get; }
        public FpsRender FpsRender { get; }
        public InformationMonitor InformationMonitor { get; }
        public AudioMonitor AudioMonitor { get; }
        public AudioRender AudioRender { get; }
        public RamMonitor RamMonitor { get; }
        public RamRender RamRender { get; }
    }
}