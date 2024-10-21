using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct FieldTask<TValue>
    {
        public Label label;
        public ValueContainer<TValue> data;
        public Rect screen;
    }
}