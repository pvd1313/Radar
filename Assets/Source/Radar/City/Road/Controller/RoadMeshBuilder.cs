using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Gizmo;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Beast.Radar.City
{
    [StaticUtility]
    public static class RoadMeshBuilder
    {
        public static void GetMeshSize(int roadPointCount, out int vertexCount, out int triangleCount)
        {
            if (roadPointCount < 2)
            {
                throw new ArgumentException("roadPointCount must be >= 2");
            }
            
            vertexCount = roadPointCount * 2;
            triangleCount = (roadPointCount - 1) * 2;
        }
        
        public static void BuildAllRoadMesh(RoadSplineMap roadSplineMap, Mesh roadMesh, float segmentLength, IGizmoHandle gizmo)
        {
            if (segmentLength < 0.1f)
            {
                throw new ArgumentException($"segmentLength is too small ({segmentLength})");
            }

            int totalPointCount = 0;

            NativeArray<int> pointCount = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            NativeArray<int> pointOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);

            for (int i = 0; i < roadSplineMap.roads.Length; i++)
            {
                RoadSpline road = roadSplineMap.roads[i];

                int roadPointCount = ContinuousRoadSplinePointComputer.ComputeSplinePointCount(road.length, segmentLength);

                pointCount[i] = roadPointCount;
                pointOffset[i] = totalPointCount;

                totalPointCount += roadPointCount;
            }

            using NativeArray<float2> roadPoints = BNative.TempJobArray<float2>(totalPointCount);

            ContinuousRoadSplinePointsJob splinePointsJob = new()
            {
                inSegmentLength = segmentLength,
                inPointCount = pointCount,
                inPointsOffset = pointOffset,
                inRoadMap = roadSplineMap,
                inPrecisionMode = EPrecisionMode.Float,
                outPoints = roadPoints
            };

            Profiler.BeginSample("ContinuousRoadSplinePointsJob");
            splinePointsJob.Schedule(roadSplineMap.roads.Length, 8).Complete();
            Profiler.EndSample();
            
            int vertexCount = 0;
            int triangleCount = 0;
            NativeArray<int> vertexOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            NativeArray<int> triangleOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            for (int i = 0; i < roadSplineMap.roads.Length; i++)
            {
                int roadPointCount = pointCount[i];

                vertexOffset[i] = vertexCount;
                triangleOffset[i] = triangleCount * 3;
                
                GetMeshSize(roadPointCount, out int roadVertexCount, out int roadTriangleCount);

                vertexCount += roadVertexCount;
                triangleCount += roadTriangleCount;
            }

            using NativeArray<float3> vertices = BNative.TempJobArray<float3>(vertexCount);
            using NativeArray<float3> normals = BNative.TempJobArray<float3>(vertexCount); 
            using NativeArray<int> triangles = BNative.TempJobArray<int>(triangleCount * 3);

            RoadMeshJob meshJob = new()
            {
                inPoints = roadPoints,
                inPointCount = pointCount,
                inPointOffset = pointOffset,
                inTriangleOffset = triangleOffset,
                inVertexOffset = vertexOffset,
                outTriangles = triangles,
                outVertices = vertices,
                outNormals = normals
            };
            
            Profiler.BeginSample("RoadMeshJob");
            meshJob.Schedule(roadSplineMap.roads.Length, 8).Complete();
            Profiler.EndSample();

            pointOffset.Dispose();
            pointCount.Dispose();
            triangleOffset.Dispose();
            vertexOffset.Dispose();
            
            roadMesh.Clear();
            roadMesh.SetVertices(vertices, 0, vertices.Length, MeshUpdateFlags.DontRecalculateBounds);
            roadMesh.SetNormals(normals);
            roadMesh.SetIndices(triangles, MeshTopology.Triangles, 0, false);
        }

        public static void BuildMesh(RoadMeshTask task)
        {
            float2 center = task.inPoints[0];
            float2 right = task.inPoints[1];
            float2 left = center + BMath.Normalize(center - right);

            int triangleOffset = 0;
            int vertexOffset = 0;

            _writeMiter(task, left, center, right, ref vertexOffset);

            for (int i = 1; i < task.inPoints.Length - 1; i++)
            {
                left = task.inPoints[i - 1];
                center = task.inPoints[i];
                right = task.inPoints[i + 1];
                
                _writeQuad(task, vertexOffset, ref triangleOffset);
                
                _writeMiter(task, left, center, right, ref vertexOffset);
            }
            
            left = task.inPoints[^2];
            center = task.inPoints[^1];
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