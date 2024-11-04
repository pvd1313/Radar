namespace Beast.Radar.Terrain
{
    public interface IShortTextureConverter<out TValue>
    {
        TValue Convert(short pixel);
    }
}