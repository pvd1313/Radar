using System.Collections.Generic;
using System.Diagnostics;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
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
                gizmo.SetColor(Color.red);
                
                for (int i = 0; i < road.pointCount; i++)
                {
                    gizmo.AddPoint(roadMap.roadPoints[road.startPointIndex + i], 4);
                }
                
                gizmo.SetColor(Color.gray);
                
                for (int i = 0; i < road.pointCount - 1; i++)
                {
                    gizmo.AddPoint(roadMap.roadHandles[road.startPointIndex + i], 4);
                }
            }
        }

        [Conditional("DEVELOPMENT")]
        public static void DrawRoadSpline(IGizmoHandle gizmo, RoadSplineMap roadMap, int roadIndex, int pointsPerCurve)
        {
            gizmo.Clear();

            RoadSpline road = roadMap.roads[roadIndex];
            
            gizmo.SetColor(Color.red);
                
            for (int i = 0; i < road.pointCount; i++)
            {
                gizmo.AddPoint(roadMap.roadPoints[road.startPointIndex + i], 4);
            }
                
            gizmo.SetColor(Color.gray);
                
            for (int i = 0; i < road.pointCount - 1; i++)
            {
                gizmo.AddPoint(roadMap.roadHandles[road.startPointIndex + i], 4);
            }
            
            gizmo.SetColor(Color.blue);

            for (int splineIndex = 0; splineIndex < road.pointCount; splineIndex++)
            {
                BezierCurve2D curve = new ()
                {
                    point0 = roadMap.roadPoints[road.startPointIndex + splineIndex],
                    point1 = roadMap.roadPoints[road.startPointIndex + splineIndex + 1],
                    handle0 = roadMap.roadHandles[road.startPointIndex + splineIndex],
                    handle1 = roadMap.roadHandles[road.startPointIndex + splineIndex]
                };

                for (int i = 0; i < pointsPerCurve; i++)
                {
                    float t = (i + 1.0f) / (pointsPerCurve + 1.0f);

                    float2 point = curve.ComputePoint(t);

                    gizmo.AddPoint(point, 2);
                }
            }
        }
    }
}