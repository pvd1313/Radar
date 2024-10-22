using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct FieldTask<TValue>
    {
        public Rect screen;
        public Label label;
        public TValue value;
    }
}