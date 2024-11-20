using Beast.Claw.Editor.UI.UI;
using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public struct FieldTask<TValue>
    {
        public Rect screen;
        public Label label;
        public TValue value;
    }
}