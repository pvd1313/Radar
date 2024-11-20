using Unity.Mathematics;

namespace Beast.Claw.Editor.UI
{
    public class Float2Field : IFieldDrawer<float2>
    {
        private RectUI _ui;

        public void Warmup(RectUI ui, UIStyleGroup style)
        {
            _ui = ui;
        }

        public float2 Draw(FieldTask<float2> field)
        {
            _ui.TempTaskWithDefaultStyle(out Float2Task task);

            task.mainLabel = field.label;
            task.label0 = "x";
            task.label1 = "y";
            task.value0 = field.value.x;
            task.value1 = field.value.y;
            
            _ui.Draw(field.screen, task);

            return new float2(task.value0, task.value1);
        }
    }
}