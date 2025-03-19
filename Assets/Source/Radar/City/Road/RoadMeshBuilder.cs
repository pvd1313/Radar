using System;
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
            float2 center = task.inPoints[0] + new float2(0.5f);
            float2 right = task.inPoints[1] + new float2(0.5f);;
            float2 left = center + BMath.Normalize(center - right);

            int triangleOffset = 0;
            int vertexOffset = 0;

            _writeMiter(task, left, center, right, ref vertexOffset);

            for (int i = 1; i < task.inPoints.Length - 1; i++)
            {
                left = task.inPoints[i - 1] + new float2(0.5f);
                center = task.inPoints[i] + new float2(0.5f);
                right = task.inPoints[i + 1] + new float2(0.5f);
                
                _writeQuad(task, vertexOffset, ref triangleOffset);
                
                _writeMiter(task, left, center, right, ref vertexOffset);
            }
            
            left = task.inPoints[^2] + new float2(0.5f);
            center = task.inPoints[^1] + new float2(0.5f);
            right = center + BMath.Normalize(center - left);

            _writeQuad(task, vertexOffset, ref triangleOffset);
            
            _writeMiter(task, left, center, right, ref vertexOffset);
        }

        private static void _writeQuad(RoadMeshTask task, int vertexOffset, ref int triangleOffset)
        {
            task.outTriangleIndices[triangleOffset + 0] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 1] = vertexOffset - 1 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 2] = vertexOffset + 1 + task.inFirstVertexIndex;

            task.outTriangleIndices[triangleOffset + 3] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 4] = vertexOffset + 1 + task.inFirstVertexIndex;
            task.outTriangleIndices[triangleOffset + 5] = vertexOffset + 0 + task.inFirstVertexIndex;

            triangleOffset += 6;
        }

        private static void _writeMiter(
            RoadMeshTask task, 
            float2 leftPoint, 
            float2 centerPoint, 
            float2 rightPoint, 
            ref int vertexOffset)
        {
            float3 a = new (leftPoint.x, leftPoint.y, centerPoint.x);
            float3 b = new(centerPoint.y, rightPoint.x, rightPoint.y);

            task.outVertices[vertexOffset + 0] = a;
            task.outVertices[vertexOffset + 1] = a;
            task.outNormals[vertexOffset + 0] = b;
            task.outNormals[vertexOffset + 1] = b;

            vertexOffset += 2;
        }
    }
}