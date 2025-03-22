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
        [ReadOnly] public NativeArray<int> inTriangleOffset;

        [NativeDisableParallelForRestriction] public NativeArray<float3> outVertices;
        [NativeDisableParallelForRestriction] public NativeArray<float3> outNormals;
        [NativeDisableParallelForRestriction] public NativeArray<int> outTriangles;
        
        public void Execute(int pointGroupIndex)
        {
            int currentPointCount = inPointCount[pointGroupIndex];
            int vertexOffset = inVertexOffset[pointGroupIndex];
            int triangleOffset = inTriangleOffset[pointGroupIndex];
            
            RoadMeshBuilder.GetMeshSize(currentPointCount, out int currentVertexCount, out int currentTriangleCount);
            
            RoadMeshBuilder.BuildMesh(new RoadMeshTask
            {
                inPoints = inPoints.Slice(inPointOffset[pointGroupIndex], currentPointCount),
                inFirstVertexIndex = vertexOffset,
                outVertices = outVertices.Slice(vertexOffset, currentVertexCount),
                outNormals = outNormals.Slice(vertexOffset, currentVertexCount),
                outTriangleIndices = outTriangles.Slice(triangleOffset, currentTriangleCount * 3)
            });
        }
    }
}