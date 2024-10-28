namespace Beast.Radar.GameTime
{
    public interface IWorldTime
    {
        float Minutes { get; }
        float GetHours();
    }
}