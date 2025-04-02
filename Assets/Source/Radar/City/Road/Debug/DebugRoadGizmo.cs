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
    public static class DebugRoadGizmo
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
                    gizmo.AddScreenPoint(startPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                if (junctionPoints.Add(endPoint))
                {
                    gizmo.AddScreenPoint(endPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                gizmo.SetColor(Color.black);

                for (int i = 1; i < road.pointCount - 1; i++)
                {
                    ushort2 point = roadMap.roadPoints[road.startPointIndex + i];
                    
                    gizmo.AddScreenPoint(point + new float2(0.5f, 0.5f), 4f);
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
            gizmo.Clear();

            RoadSpline road = roadMap.roads[roadIndex];
            
            _drawSplineControlPoints(gizmo, roadMap, road);

            RoadContinuousSplineTask task = new ()
            {
                inRoadIndex = roadIndex,
                inRoadMap = roadMap,
                inSegmentLength = segmentLength,
                inPrecision = EPrecisionMode.Double,
                inRoadPointCount = RoadContinuousSplineUtil.GetPointCount(road.length, segmentLength)
            };

            using NativeArray<float2> points = BNative.TempJobArray<float2>(task.inRoadPointCount);

            task.outPoints = points;

            RoadContinuousSplineUtil.Build(task);
            
            gizmo.SetColor(Color.blue);

            foreach (float2 point in points)
            {
                gizmo.AddScreenPoint(point);
            }
        }

        private static void _drawSplineControlPoints(IGizmoHandle gizmo, RoadSplineMap roadMap, RoadSpline road)
        {
            gizmo.SetColor(Color.red);
                
            for (int i = 0; i < road.pointCount; i++)
            {
                gizmo.AddScreenPoint(roadMap.roadPoints[road.firstPointIndex + i], 8);
            }
                
            gizmo.SetColor(Color.gray);
                
            for (int i = 0; i < road.pointCount - 1; i++)
            {
                gizmo.AddScreenPoint(roadMap.roadHandles[road.firstPointIndex + i], 4);
            }
        }
    }
}