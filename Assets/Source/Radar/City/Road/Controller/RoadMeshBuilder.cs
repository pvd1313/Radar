using System;
using System.Collections.Generic;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
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

        public static Mesh BuildGlobalRoadMesh(
            IController owner,
            string meshName,
            RoadSplineMap roadSplineMap, 
            float segmentLength,
            EPrecisionMode precision)
        {
            Mesh mesh = BGraphic.CreateMesh(owner);
            mesh.name = meshName;
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
            
            int totalPointCount = 0;

            NativeArray<int> pointCount = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            NativeArray<int> pointOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);

            for (int i = 0; i < roadSplineMap.roads.Length; i++)
            {
                RoadSpline road = roadSplineMap.roads[i];

                int roadPointCount = RoadContinuousSplineUtil.GetPointCount(road.length, segmentLength);

                pointCount[i] = roadPointCount;
                pointOffset[i] = totalPointCount;

                totalPointCount += roadPointCount;
            }

            using NativeArray<float2> roadPoints = BNative.TempJobArray<float2>(totalPointCount);

            RoadContinuousSplinePointsJob splinePointsJob = new()
            {
                inSegmentLength = segmentLength,
                inPointCount = pointCount,
                inPointsOffset = pointOffset,
                inRoadMap = roadSplineMap,
                inPrecision = precision,
                outPoints = roadPoints
            };

            Profiler.BeginSample("ContinuousRoadSplinePointsJob");
            splinePointsJob.Schedule(roadSplineMap.roads.Length, 8).Complete();
            Profiler.EndSample();
            
            int vertexCount = 0;
            int triangleCount = 0;
            NativeArray<int> vertexOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            NativeArray<int> triangleOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            NativeArray<int> vertexIndexOffset = BNative.TempJobArray<int>(roadSplineMap.roads.Length);
            for (int i = 0; i < roadSplineMap.roads.Length; i++)
            {
                int roadPointCount = pointCount[i];

                vertexOffset[i] = vertexCount;
                triangleOffset[i] = triangleCount * 3;
                vertexIndexOffset[i] = vertexCount;
                
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
                inVertexIndexOffset = vertexIndexOffset,
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
            vertexIndexOffset.Dispose();
            
            mesh.Clear();
            mesh.SetVertices(vertices, 0, vertices.Length, MeshUpdateFlags.DontRecalculateBounds);
            mesh.SetNormals(normals);
            mesh.SetIndices(triangles, MeshTopology.Triangles, 0, false);

            return mesh;
        }

        public static Dictionary<int2, Mesh> BuildRoadGridMeshes(
            IController owner,
            RoadSplineMap roadMap,
            string meshNamePrefix,
            CellGrid<int> roadGrid, 
            float terrainSize,
            float segmentLength,
            EPrecisionMode precision)
        {
            const int MAX_SLICE_PER_CELL = 16;
            
            int cellCountX = BMath.CeilToInt(terrainSize / roadGrid.cellSize);
            int cellTotal = cellCountX * cellCountX;

            Profiler.BeginSample("Prepare cellBounds");
            NativeArray<Bounds2D> cellBounds = BNative.TempJobArray<Bounds2D>(cellTotal);
            for (int i = 0; i < cellTotal; i++)
            {
                int2 cellIndex = new (i % cellCountX, i / cellCountX);

                float xMin = cellIndex.x * roadGrid.cellSize;
                float yMin = cellIndex.y * roadGrid.cellSize;
                
                cellBounds[i] = new Bounds2D(xMin, xMin + roadGrid.cellSize, yMin, yMin + roadGrid.cellSize);
            }
            Profiler.EndSample();
            
            RoadSliceSplinesJob sliceJob = new()
            {
                inSegmentLength = segmentLength,
                inRoadMap = roadMap,
                inRoadGrid = roadGrid,
                inMaxSliceCount = MAX_SLICE_PER_CELL,
                inBounds = cellBounds,
                outSlicePointCount = BNative.TempJobArray<int>(cellTotal * MAX_SLICE_PER_CELL),
                outSlices = BNative.TempJobArray<RoadSplineSlice>(cellTotal * MAX_SLICE_PER_CELL),
                outSliceCount = BNative.TempJobArray<int>(cellTotal),
            };
            
            Profiler.BeginSample("RoadSliceSplinesJob");
            sliceJob.Schedule(cellTotal, 16).Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Compute sliceCountTotal");
            int sliceCountTotal = 0;
            for (int i = 0; i < cellTotal; i++)
            {
                sliceCountTotal += sliceJob.outSliceCount[i];
            }
            Profiler.EndSample();

            NativeArray<int> pointOffsets = BNative.TempJobArray<int>(sliceCountTotal);
            NativeArray<int2> cellIndices = BNative.TempJobArray<int2>(sliceCountTotal);
            NativeArray<int> pointCounts = BNative.TempJobArray<int>(sliceCountTotal);
            NativeArray <RoadSplineSlice> slices = BNative.TempJobArray<RoadSplineSlice>(sliceCountTotal);
            
            Profiler.BeginSample("Prepare points buffer");
            int sliceIndex = 0;
            int estimatedPointCount = 0;
            int meshCount = 0;
            for (int i = 0; i < cellTotal; i++)
            {
                int sliceCount = sliceJob.outSliceCount[i];
                if (sliceCount == 0)
                {
                    continue;
                }

                int2 cellIndex = new (i % cellCountX, i / cellCountX);
                for (int j = 0; j < sliceCount; j++)
                {
                    int slicePointCount = sliceJob.outSlicePointCount[i * MAX_SLICE_PER_CELL + j];

                    pointCounts[sliceIndex] = slicePointCount;
                    pointOffsets[sliceIndex] = estimatedPointCount;
                    cellIndices[sliceIndex] = cellIndex;
                    slices[sliceIndex] = sliceJob.outSlices[i * MAX_SLICE_PER_CELL + j];

                    sliceIndex++;
                    estimatedPointCount += slicePointCount;
                }

                meshCount++;
            }
            Profiler.EndSample();

            RoadSlicedSplinePointsJob pointsJob = new() 
            {
                inRoadMap = roadMap,
                inSegmentLength = segmentLength,
                inPrecision = precision,
                inRoadSlices = slices,
                inPointCount = pointCounts,
                inPointOffset = pointOffsets,
                outPoints = BNative.TempJobArray<float2>(estimatedPointCount),
                outPointCount = BNative.TempJobArray<int>(sliceCountTotal)
            };
            
            Profiler.BeginSample("RoadSlicedSplinePointsJob");
            pointsJob.Schedule(sliceCountTotal, 8).Complete();
            Profiler.EndSample();

            NativeArray<int> vertexOffsets = BNative.TempJobArray<int>(sliceCountTotal);
            NativeArray<int> triangleOffsets = BNative.TempJobArray<int>(sliceCountTotal);
            NativeArray<int> vertexIndexOffsets = BNative.TempJobArray<int>(sliceCountTotal);

            Profiler.BeginSample("Prepare vertex buffer");
            int totalVertexCount = 0;
            int totalTriangleCount = 0;
            int vertexIndex = 0;
            int2 prevCell = new (int.MaxValue, int.MaxValue);
            for (int i = 0; i < sliceCountTotal; i++)
            {
                int pointCount = pointsJob.outPointCount[i];
                
                GetMeshSize(pointCount, out int vertexCount, out int triangleCount);

                vertexOffsets[i] = totalVertexCount;
                triangleOffsets[i] = totalTriangleCount * 3;

                int2 cellIndex = cellIndices[i];

                if (cellIndex.Equals(prevCell))
                {
                    vertexIndexOffsets[i] = vertexIndex;
                }
                else
                {
                    vertexIndexOffsets[i] = 0;
                    vertexIndex = 0;
                }

                vertexIndex += vertexCount;
                totalVertexCount += vertexCount;
                totalTriangleCount += triangleCount;
                prevCell = cellIndex;
            }
            Profiler.EndSample();

            using NativeArray<float3> vertices = BNative.TempJobArray<float3>(totalVertexCount);
            using NativeArray<float3> normals = BNative.TempJobArray<float3>(totalVertexCount);
            using NativeArray<int> triangles = BNative.TempJobArray<int>(totalTriangleCount * 3);
            
            RoadMeshJob meshJob = new()
            {
                inPoints = pointsJob.outPoints,
                inPointOffset = pointOffsets,
                inPointCount = pointsJob.outPointCount,
                inVertexIndexOffset = vertexIndexOffsets,
                outVertices = vertices,
                outNormals = normals,
                outTriangles = triangles,
                inVertexOffset = vertexOffsets,
                inTriangleOffset = triangleOffsets
            };
            
            Profiler.BeginSample("RoadMeshJob");
            meshJob.Schedule(sliceCountTotal, 8).Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Create meshes");
            
            Dictionary<int2, Mesh> gridMeshes = new(meshCount);

            for (int i = 0; i < sliceCountTotal;)
            {
                int2 cellIndex = cellIndices[i];
                int vertexOffset = vertexOffsets[i];
                int triangleOffset = triangleOffsets[i];
                
                GetMeshSize(pointsJob.outPointCount[i], out int meshVertexCount, out int meshTriangleCount);

                int j = i + 1;

                for (; j < sliceCountTotal && cellIndices[j].Equals(cellIndex); j++)
                {
                    GetMeshSize(pointsJob.outPointCount[j], out int vertexCount, out int triangleCount);

                    meshVertexCount += vertexCount;
                    meshTriangleCount += triangleCount;
                }

                Mesh mesh = BGraphic.CreateMesh(owner);
                mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
                mesh.name = meshNamePrefix + " (" + cellIndex.x + "," + cellIndex.y + ")";
                
                mesh.SetVertices(vertices, vertexOffset, meshVertexCount, MeshUpdateFlags.DontRecalculateBounds);
                mesh.SetNormals(normals, vertexOffset, meshVertexCount);
                mesh.SetIndices(triangles, triangleOffset, meshTriangleCount * 3, MeshTopology.Triangles, 0, false);

                gridMeshes[cellIndex] = mesh;

                i = j;
            }
            Profiler.EndSample();

            vertexOffsets.Dispose();
            triangleOffsets.Dispose();
            pointOffsets.Dispose();
            pointCounts.Dispose();
            vertexIndexOffsets.Dispose();
            cellBounds.Dispose();
            slices.Dispose();
            cellIndices.Dispose();

            pointsJob.outPoints.Dispose();
            pointsJob.outPointCount.Dispose();
                
            sliceJob.outSlices.Dispose();
            sliceJob.outSliceCount.Dispose();
            sliceJob.outSlicePointCount.Dispose();

            return gridMeshes;
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
            task.outTriangles[triangleOffset + 0] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangles[triangleOffset + 1] = vertexOffset - 1 + task.inFirstVertexIndex;
            task.outTriangles[triangleOffset + 2] = vertexOffset + 1 + task.inFirstVertexIndex;

            task.outTriangles[triangleOffset + 3] = vertexOffset - 2 + task.inFirstVertexIndex;
            task.outTriangles[triangleOffset + 4] = vertexOffset + 1 + task.inFirstVertexIndex;
            task.outTriangles[triangleOffset + 5] = vertexOffset + 0 + task.inFirstVertexIndex;

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