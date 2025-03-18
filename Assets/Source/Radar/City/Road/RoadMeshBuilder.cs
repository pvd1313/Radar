using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public static class RoadMeshBuilder
    {
        public static void GetMeshSize(
            int roadPointCount, 
            ERoadMeshBuildingMode mode, 
            int modeValue,
            out int vertexCount,
            out int triangleCount)
        {
            if (roadPointCount < 2)
            {
                throw new ArgumentException("roadPointCount must be >= 2");
            }

            switch (mode)
            {
                case ERoadMeshBuildingMode.SkipPoints:
                    if (modeValue < 1)
                    {
                        throw new ArgumentException("modeValue must be >= 1 when skipping points");
                    }
                    roadPointCount = (roadPointCount - 2) / modeValue + 2;
                    break;
                
                case ERoadMeshBuildingMode.ExtraPoints:
                    if (modeValue < 0)
                    {
                        throw new AggregateException("modeValue must be >= 0 when adding extra points");
                    }
                    roadPointCount += (roadPointCount - 1) * modeValue;
                    break;
                
                default:
                    throw new ArgumentException("Invalid mode: " + mode);
            }
            
            vertexCount = roadPointCount * 2;
            triangleCount = (roadPointCount - 1) * 2;
        }

        public static void BuildMesh(RoadMeshTask task)
        {
            switch (task.inMode)
            {
                case ERoadMeshBuildingMode.SkipPoints:
                    _buildMeshSkipPoints(task);
                    return;
                
                case ERoadMeshBuildingMode.ExtraPoints:
                    throw new NotImplementedException();
                
                default:
                    throw new InvalidOperationException();
            }
        }

        private static void _buildMeshSkipPoints(RoadMeshTask task)
        {
            // v1   v3
            // p0   p1
            // v0   v2
            float3 point0 = _ushortPointToFloat(task.inPoints[0]);
            float3 point1 = _ushortPointToFloat(task.inPoints[1]);
            float3 normal0 = _getNormal(point0, point1);
            float3 normal1 = normal0;
            
            task.outVertices[0] = point0;
            task.outVertices[1] = point0;

            task.outNormals[0] = normal0 * -1f;
            task.outNormals[1] = normal0;
            
            int vertexOffset = 2;
            int triangleOffset = 0;

            for (int i = 1; i < task.inPoints.Length - 1; i++)
            {
                if (i % task.inModeValue != 0)
                {
                    continue;
                }

                (point0, point1) = (point1, _ushortPointToFloat(task.inPoints[i + 1]));

                normal1 = _getNormal(point0, point1);

                task.outVertices[vertexOffset + 0] = point0;
                task.outVertices[vertexOffset + 1] = point0;

                float3 averageNormal = BMath.Normalize(normal0 + normal1);

                task.outNormals[vertexOffset + 0] = averageNormal * -1f;
                task.outNormals[vertexOffset + 1] = averageNormal;

                task.outTriangleIndices[triangleOffset + 0] = vertexOffset - 2 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 1] = vertexOffset - 1 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 2] = vertexOffset + 1 + task.inFirstVertexIndex;

                task.outTriangleIndices[triangleOffset + 3] = vertexOffset - 2 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 4] = vertexOffset + 1 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 5] = vertexOffset + 0 + task.inFirstVertexIndex;

                vertexOffset += 2;
                triangleOffset += 6;
                normal0 = normal1;
            }

            task.outVertices[vertexOffset + 0] = point1;
            task.outVertices[vertexOffset + 1] = point1;

            task.outNormals[vertexOffset + 0] = normal1 * -1f;
            task.outNormals[vertexOffset + 1] = normal1;
            
            task.outTriangleIndices[triangleOffset + 0] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 1] = vertexOffset - 1 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 2] = vertexOffset + 1 + task.inFirstVertexIndex;

            task.outTriangleIndices[triangleOffset + 3] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 4] = vertexOffset + 1 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 5] = vertexOffset + 0 + task.inFirstVertexIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 _ushortPointToFloat(ushort2 point)
        {
            return new float3(point.x + 0.5f, point.y + 0.5f, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 _getNormal(float3 point0, float3 point1)
        {
            float3 direction = BMath.Normalize(point1 - point0);

            return new float3(direction.y * -1.0f, direction.x, 0);
        }
    }
}