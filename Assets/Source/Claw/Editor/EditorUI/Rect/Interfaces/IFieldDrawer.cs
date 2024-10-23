namespace Beast.Claw.Editor
{
    public interface IFieldDrawer<TValue>
    {
        public void Warmup(RectUI ui);

        public TValue Draw(FieldTask<TValue> field);
    }
}