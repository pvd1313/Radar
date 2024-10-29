namespace Beast.Radar.Player
{
    public interface IPlayerState
    {
        void SetState<TValue>(string tag, TValue value);
        bool TryGetState<TValue>(string tag, out TValue value);
    }
}