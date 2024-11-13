using Beast.Claw.UI;
using Noesis;

namespace Beast.Radar.UI
{
    public class CanvasView : BeastControl
    {
        public Grid grid;

        public override void BindElements()
        {
            _find("grid", out grid);
        }
    }
}
