using Beast.Claw.Math;
using Unity.Collections;

namespace Beast.Radar.City
{
    public struct RoadInfo
    {
        public ushort2 startPoint;
        public ushort2 endPoint;
        public NativeSlice<ushort2> points;
        public Bounds2D bounds;
    }
}