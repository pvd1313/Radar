using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadSplineTask
    {
        public RoadSplineMap inRoadMap;
        public int inRoadIndex;
        public float inSegmentLength;
        public EPrecisionMode precisionMode;
        
        public NativeSlice<float2> outPoints;
    }
}