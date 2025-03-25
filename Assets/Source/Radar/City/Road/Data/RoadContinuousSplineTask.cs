using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadContinuousSplineTask
    {
        public RoadSplineMap inRoadMap;
        public int inRoadIndex;
        public float inSegmentLength;
        public EPrecisionMode inPrecision;
        public int inRoadPointCount;
        
        public NativeSlice<float2> outPoints;
    }
}