using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public interface IGizmosDrawer : IController
    {
        public Color GizmosColor { get; }
        
        void Draw(GizmosHandle handle);
    }
}