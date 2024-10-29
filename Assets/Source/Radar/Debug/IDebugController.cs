namespace Beast.Radar.Debug
{
    public interface IDebugController
    {
        void SetElementState(DebugElementID id, bool isActive);
        bool GetElementState(DebugElementID id);
    }
}