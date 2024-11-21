using Beast.Claw.Math;
using Unity.Mathematics;

namespace Beast.Claw.Editor.UI
{
    public class CommonDrawersContainer
    {
        public void BindAllTo(RectUI ui)
        {
            ui.BindDrawer<Float2Drawer, Float2Task>();
            
            ui.BindField<Float2Field, float2>();
            ui.BindField<FloatMinMaxField, FloatMinMax>();
            ui.BindField<Int2Field, int2>();
        }
    }
}