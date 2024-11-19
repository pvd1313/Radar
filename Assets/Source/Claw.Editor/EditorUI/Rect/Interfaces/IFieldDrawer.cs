namespace Beast.Claw.Editor
{
    public interface IFieldDrawer<TValue>
    {
        public void Warmup(RectUI ui, UIStyleGroup style);

        public TValue Draw(FieldTask<TValue> field);
    }
}