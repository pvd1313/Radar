using Beast.Claw.Editor.Drawers;
using Beast.Claw.Math;

namespace Beast.Claw.Editor.Fields
{
    public class FloatMinMaxField : IFieldDrawer<FloatMinMax>
    {
        private RectUI _ui;

        public void Initialize(RectUI ui)
        {
            _ui = ui;
        }

        public void Draw(FieldTask<FloatMinMax> field)
        {
            _ui.TempTask(out Float2Task task);

            task.SetStyle(_ui.DefaultStyleSet);
            task.mainLabel = field.label;
            task.label0 = _ui.TempLabel("min");
            task.label1 = _ui.TempLabel("max");
            task.value0 = field.data.value.min;
            task.value1 = field.data.value.max;

            _ui.DrawRect(field.screen, task);

            field.data.value.min = task.value0;
        }
    }
}