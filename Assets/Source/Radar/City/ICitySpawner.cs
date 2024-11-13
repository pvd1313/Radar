using Unity.Collections;

namespace Beast.Radar.City
{
    public interface ICitySpawner
    {
        public NativeList<CityInfo> Cities { get; }
    }
}