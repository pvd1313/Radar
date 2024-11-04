namespace Beast.Radar.Debug
{
    public interface IDebugController
    {
        void SetElementState(EDebugFeature id, bool isActive);
        bool GetElementState(EDebugFeature id);
    }
}