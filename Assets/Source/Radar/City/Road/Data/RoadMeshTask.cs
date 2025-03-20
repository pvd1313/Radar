using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadMeshTask
    {
        public NativeSlice<ushort2> inPoints;
        public ERoadMeshBuildingMode inMode;
        public int inModeValue;
        public int inFirstVertexIndex;
        
        public NativeSlice<float3> outVertices;
        public NativeSlice<float3> outNormals;
        public NativeSlice<float2> outUVs;
        public NativeSlice<int> outTriangleIndices;
    }
}