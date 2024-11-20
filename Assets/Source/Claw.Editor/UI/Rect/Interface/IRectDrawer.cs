using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public interface IRectDrawer<in TDrawTask> where TDrawTask : class, new()
    {
        public void Warmup(RectUI ui, UIStyleGroup style);
        
        public void Draw(Rect screen, TDrawTask task);
    }
}