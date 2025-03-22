using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct RoadMeshTask
    {
        public NativeSlice<float2> inPoints;
        public int inFirstVertexIndex;
        
        public NativeSlice<float3> outVertices;
        public NativeSlice<float3> outNormals;
        public NativeSlice<int> outTriangleIndices;
    }
}