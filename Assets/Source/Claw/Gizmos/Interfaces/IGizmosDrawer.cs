using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public interface IGizmosDrawer
    {
        public Color GizmosColor { get; }
        void Draw(GizmosHandle handle);
    }
}