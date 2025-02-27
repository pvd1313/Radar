using Unity.Collections;

namespace Beast.Radar.City
{
    public interface ICitySpawner
    {
        public NativeSlice<CityInfo> Cities { get; }
    }
}