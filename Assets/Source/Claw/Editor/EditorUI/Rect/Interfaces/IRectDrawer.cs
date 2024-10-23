using UnityEngine;

namespace Beast.Claw.Editor
{
    public interface IRectDrawer<in TDrawTask> where TDrawTask : class, new()
    {
        public void Warmup(RectUI ui);
        
        public void Draw(Rect screen, TDrawTask task);
    }
}