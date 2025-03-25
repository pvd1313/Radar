using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    [BurstCompile]
    public struct RoadMeshJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float2> inPoints;
        [ReadOnly] public NativeArray<int> inPointCount;
        [ReadOnly] public NativeArray<int> inPointOffset;
        [ReadOnly] public NativeArray<int> inVertexOffset;
        [ReadOnly] public NativeArray<int> inVertexIndexOffset;
        [ReadOnly] public NativeArray<int> inTriangleOffset;

        [NativeDisableParallelForRestriction] public NativeArray<float3> outVertices;
        [NativeDisableParallelForRestriction] public NativeArray<float3> outNormals;
        [NativeDisableParallelForRestriction] public NativeArray<int> outTriangles;
        
        public void Execute(int jobIndex)
        {
            int currentPointCount = inPointCount[jobIndex];
            int vertexOffset = inVertexOffset[jobIndex];
            int triangleOffset = inTriangleOffset[jobIndex];
            
            RoadMeshBuilder.GetMeshSize(currentPointCount, out int currentVertexCount, out int currentTriangleCount);
            
            RoadMeshBuilder.BuildMesh(new RoadMeshTask
            {
                inPoints = inPoints.Slice(inPointOffset[jobIndex], currentPointCount),
                inFirstVertexIndex = inVertexIndexOffset[jobIndex],
                outVertices = outVertices.Slice(vertexOffset, currentVertexCount),
                outNormals = outNormals.Slice(vertexOffset, currentVertexCount),
                outTriangleIndices = outTriangles.Slice(triangleOffset, currentTriangleCount * 3)
            });
        }
    }
}