using Beast.Claw.Editor.Drawers;
using Unity.Mathematics;

namespace Beast.Claw.Editor.Fields
{
    public class Float2Field : IFieldDrawer<float2>
    {
        private RectUI _ui;
        
        public void Initialize(RectUI ui)
        {
            _ui = ui;
        }

        public void Draw(FieldTask<float2> field)
        {
            _ui.TempTask(out Float2Task task);

            task.SetStyle(_ui.DefaultStyleSet);
            task.mainLabel = field.label;
            task.label0 = _ui.TempLabel("x");
            task.label1 = _ui.TempLabel("y");
            task.value0 = field.data.value.x;
            task.value1 = field.data.value.y;
            
            _ui.DrawRect(field.screen, task);

            field.data.value.x = task.value0;
            field.data.value.y = task.value0;
        }
    }
}