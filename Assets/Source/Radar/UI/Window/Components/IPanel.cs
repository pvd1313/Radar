using Beast.Claw.Common;

namespace Beast.Radar.UI
{
    public interface IPanel : IWindow<Empty>
    {
        void Warmup();
    }
}