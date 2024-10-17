namespace Beast.Radar.UI
{
    public interface IUIPoolEntity
    {
        void Init(params object[] parameters);
        void Enable();
        void Disable();
    }
}