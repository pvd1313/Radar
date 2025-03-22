using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct ContinuousRoadSplineTask
    {
        public RoadSplineMap inRoadMap;
        public int inRoadIndex;
        public float inSegmentLength;
        public EPrecisionMode precisionMode;
        public int inRoadPointCount;
        
        public NativeSlice<float2> outPoints;
    }
}