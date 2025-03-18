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
            float2 point0 = task.inPoints[0] + new float2(0.5f);
            float2 point1 = task.inPoints[1] + new float2(0.5f);
            float2 normal = _getNormal(point0, point1);
            
            task.outVertices[0] = point0.XY0();
            task.outVertices[1] = point0.XY0();

            task.outNormals[0] = (normal * -1f).XY0();
            task.outNormals[1] = normal.XY0();
            
            int vertexOffset = 2;
            int triangleOffset = 0;

            for (int i = 1; i < task.inPoints.Length - 1; i++)
            {
                if (i % task.inModeValue != 0)
                {
                    continue;
                }

                float2 left = task.inPoints[i - 1] + new float2(0.5f);
                float2 center = task.inPoints[i] + new float2(0.5f);
                float2 right = task.inPoints[i + 1] + new float2(0.5f);
                
                task.outVertices[vertexOffset + 0] = center.XY0();
                task.outVertices[vertexOffset + 1] = center.XY0();

                float2 dir0 = BMath.Normalize(center - left);
                float2 dir1 = BMath.Normalize(right - center);
                float2 bisector = BMath.Normalize(dir0 + dir1);
                float2 perp = new (bisector.y * -1.0f, bisector.x);
                float miterScale = 1f / BMath.Max(0.01f, BMath.Dot(perp, new float2(dir1.y * -1.0f, dir1.x)));

                normal = perp * miterScale;
                
                task.outNormals[vertexOffset + 0] = (normal * -1f).XY0();
                task.outNormals[vertexOffset + 1] = normal.XY0();

                task.outTriangleIndices[triangleOffset + 0] = vertexOffset - 2 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 1] = vertexOffset - 1 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 2] = vertexOffset + 1 + task.inFirstVertexIndex;

                task.outTriangleIndices[triangleOffset + 3] = vertexOffset - 2 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 4] = vertexOffset + 1 + task.inFirstVertexIndex;
                task.outTriangleIndices[triangleOffset + 5] = vertexOffset + 0 + task.inFirstVertexIndex;

                vertexOffset += 2;
                triangleOffset += 6;
            }
            
            point0 = task.inPoints[^2] + new float2(0.5f);
            point1 = task.inPoints[^1] + new float2(0.5f);
            normal = _getNormal(point0, point1);

            task.outVertices[vertexOffset + 0] = point1.XY0();
            task.outVertices[vertexOffset + 1] = point1.XY0();

            task.outNormals[vertexOffset + 0] = (normal * -1f).XY0();
            task.outNormals[vertexOffset + 1] = normal.XY0();
            
            task.outTriangleIndices[triangleOffset + 0] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 1] = vertexOffset - 1 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 2] = vertexOffset + 1 + task.inFirstVertexIndex;

            task.outTriangleIndices[triangleOffset + 3] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 4] = vertexOffset + 1 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 5] = vertexOffset + 0 + task.inFirstVertexIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 _getNormal(float2 point0, float2 point1)
        {
            float2 direction = BMath.Normalize(point1 - point0);
            return new float2(direction.y * -1.0f, direction.x);
        }
    }
}