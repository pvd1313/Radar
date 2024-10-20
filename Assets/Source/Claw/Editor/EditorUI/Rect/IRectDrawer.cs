using UnityEngine;

namespace Beast.Claw.Editor
{
    public interface IRectDrawer<in TDrawTask> where TDrawTask : class, new()
    {
        public void Initialize(RectUI ui);
        
        public void Draw(Rect screen, TDrawTask task);
    }
}