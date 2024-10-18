namespace Beast.Claw.Graphic
{
    public interface IRender<in TTask>
    {
        void Render(TTask task);
    }
}