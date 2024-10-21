using Beast.Claw.Editor.Fields;

namespace Beast.Claw.Editor
{
    public interface IFieldDrawer<TValue>
    {
        public void Initialize(RectUI ui);

        public void Draw(FieldTask<TValue> field);
    }
}