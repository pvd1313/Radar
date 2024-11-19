using Beast.Claw.Editor.Drawers;
using Unity.Mathematics;

namespace Beast.Claw.Editor.Fields
{
    public class Int2Field : IFieldDrawer<int2>
    {
        private RectUI _ui;

        public void Warmup(RectUI ui, UIStyleGroup style)
        {
            _ui = ui;
        }

        public int2 Draw(FieldTask<int2> field)
        {
            _ui.TempTaskWithDefaultStyle(out Float2Task task);

            task.mainLabel = field.label;
            task.label0 = "x";
            task.label1 = "y";
            task.value0 = field.value.x;
            task.value1 = field.value.y;

            _ui.Draw(field.screen, task);

            return new int2((int)task.value0, (int)task.value1);
        }
    }
}