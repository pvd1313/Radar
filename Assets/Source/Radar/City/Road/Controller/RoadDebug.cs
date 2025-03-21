using System.Collections.Generic;
using System.Diagnostics;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.City
{
    [StaticUtility]
    public static class RoadDebug
    {
        [Conditional("DEVELOPMENT")]
        public static void DrawRoadPathMap(IGizmoHandle gizmo, RoadPathMap roadMap)
        {
            gizmo.Clear();
            
            HashSet<ushort2> junctionPoints = new (1024);
            
            foreach (RoadPath road in roadMap.roads)
            {
                gizmo.SetColor(Color.red.WithAlpha(0.8f));

                ushort2 startPoint = roadMap.roadPoints[road.startPointIndex];
                ushort2 endPoint = roadMap.roadPoints[road.startPointIndex + road.pointCount - 1];
                
                if (junctionPoints.Add(startPoint))
                {
                    gizmo.AddPoint(startPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                if (junctionPoints.Add(endPoint))
                {
                    gizmo.AddPoint(endPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                gizmo.SetColor(Color.black);

                for (int i = 1; i < road.pointCount - 1; i++)
                {
                    ushort2 point = roadMap.roadPoints[road.startPointIndex + i];
                    
                    gizmo.AddPoint(point + new float2(0.5f, 0.5f), 4f);
                }
            }
        }

        [Conditional("DEVELOPMENT")]
        public static void DrawRoadSplineMap(IGizmoHandle gizmo, RoadSplineMap roadMap)
        {
            gizmo.Clear();

            foreach (RoadSpline road in roadMap.roads)
            {
                _drawSplineControlPoints(gizmo, roadMap, road);
            }
        }

        [Conditional("DEVELOPMENT")]
        public static void DrawRoadSpline(IGizmoHandle gizmo, RoadSplineMap roadMap, int roadIndex, float segmentLength)
        {
            const int LENGTH_TABLE_MULTIPLIER = 3;
            
            gizmo.Clear();

            RoadSpline road = roadMap.roads[roadIndex];

            int maxCurvePointCount = 0;
            for (int i = 0; i < road.pointCount - 1; i++)
            {
                float curveLength = roadMap.curveLengths[road.startPointIndex + i];

                int curvePointCount = BMath.CeilToInt(curveLength / segmentLength);

                maxCurvePointCount = BMath.Max(curvePointCount, maxCurvePointCount);
            }

            maxCurvePointCount++;

            NativeArray<float> lengthBuffer = BNative.TempArray<float>(maxCurvePointCount * LENGTH_TABLE_MULTIPLIER);
            
            _drawSplineControlPoints(gizmo, roadMap, road);
            
            gizmo.SetColor(Color.blue);

            float nextPointDistance = segmentLength;

            for (int splineIndex = 0; splineIndex < road.pointCount - 1; splineIndex++)
            {
                float curveLength = roadMap.curveLengths[road.startPointIndex + splineIndex];

                if (nextPointDistance > curveLength)
                {
                    nextPointDistance -= curveLength;
                    continue;
                }

                float splineOffset = nextPointDistance;
                int splinePointCount = BMath.FloorToInt((curveLength - splineOffset) / segmentLength) + 1;
                float splineRemainingDistance = curveLength - (splinePointCount - 1) * segmentLength - splineOffset;
                nextPointDistance = segmentLength - splineRemainingDistance;
                
                BezierCurve2D curve = new ()
                {
                    point0 = roadMap.roadPoints[road.startPointIndex + splineIndex],
                    point1 = roadMap.roadPoints[road.startPointIndex + splineIndex + 1],
                    handle0 = roadMap.roadHandles[road.startPointIndex + splineIndex],
                    handle1 = roadMap.roadHandles[road.startPointIndex + splineIndex]
                };

                BezierCurveLengthTable lengthTable = BezierCurveLengthTable.Create(
                    curve,
                    lengthBuffer,
                    splinePointCount * LENGTH_TABLE_MULTIPLIER,
                    EPrecisionMode.Double);

                for (int i = 0; i < splinePointCount; i++)
                {
                    float pointTime = lengthTable.FindTimeForLength(splineOffset + i * segmentLength);
                    
                    double2 point = curve.ComputePointDouble(pointTime);

                    gizmo.AddPoint((float2)point, 4f);
                }
            }

            lengthBuffer.Dispose();
        }

        private static void _drawSplineControlPoints(IGizmoHandle gizmo, RoadSplineMap roadMap, RoadSpline road)
        {
            gizmo.SetColor(Color.red);
                
            for (int i = 0; i < road.pointCount; i++)
            {
                gizmo.AddPoint(roadMap.roadPoints[road.startPointIndex + i], 8);
            }
                
            gizmo.SetColor(Color.gray);
                
            for (int i = 0; i < road.pointCount - 1; i++)
            {
                gizmo.AddPoint(roadMap.roadHandles[road.startPointIndex + i], 4);
            }
        }
    }
}