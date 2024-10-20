using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public readonly partial struct EditorUI
    {
        private readonly EditorFlow _flow;

        public EditorUI(EditorFlow flow)
        {
            _flow = flow;
        }

        public Rect AllocateRect(float2 size)
        {
            return GUILayoutUtility.GetRect(size.x, size.y);
        }
    }
}