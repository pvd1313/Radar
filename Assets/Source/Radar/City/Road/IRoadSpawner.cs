using Unity.Collections;

namespace Beast.Radar.City
{
    public interface IRoadSpawner
    {
        public NativeSlice<RoadInfo> Roads { get; }
    }
}