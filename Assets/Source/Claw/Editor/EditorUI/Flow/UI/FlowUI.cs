using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public readonly partial struct FlowUI
    {
        private readonly Flow _flow;

        public FlowUI(Flow flow)
        {
            _flow = flow;
        }

        public Rect AllocateRect(float2 size)
        {
            return GUILayoutUtility.GetRect(size.x, size.y);
        }
    }
}