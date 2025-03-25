using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadSlicedSplineTask
    {
        public RoadSplineMap inRoadMap;
        public RoadSplineSlice inRoadSlice;
        public float inSegmentLength;
        public EPrecisionMode inPrecision;
        public int inRoadPointCount;

        public NativeSlice<float2> outPoints;
    }
}