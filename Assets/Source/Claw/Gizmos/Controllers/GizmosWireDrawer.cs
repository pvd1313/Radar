using System.Runtime.CompilerServices;
using Beast.Claw.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    internal static class GizmosWireDrawer
    {
        public static void BeginDraw()
        {
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);
        }

        public static void EndDraw()
        {
            GL.End();
            GL.PopMatrix();
        }
        
        public static void DrawLines(ShapeStorage<Line> lineStorage)
        {
            NativeArray<Line> lines = lineStorage.GetShapes(out int lineCount);
            for (int i = 0; i < lineCount; i++)
            {
                Line line = lines[i];
                GL.Color(line.color);
                GL_Line(line.start, line.end);
            }
        }

        public static void DrawRects(ShapeStorage<Rect> rectStorage)
        {
            NativeArray<Rect> rects = rectStorage.GetShapes(out int rectCount);
            for (int i = 0; i < rectCount; i++)
            {
                Rect rect = rects[i];
                GL.Color(rect.color);
                float2 center = rect.center;
                float2 halfSize = new float2(rect.size.x / 2f, rect.size.y / 2f);
                BMath.SinCos(rect.rotationRad, out float sin, out float cos);
                float2 p0 = center + BMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
                float2 p1 = center + BMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
                float2 p2 = center + BMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
                float2 p3 = center + BMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
                GL_Quad(p0, p1, p2, p3);
            }
        }

        public static void DrawSquares(ShapeStorage<Square> rectStorage)
        {
            NativeArray<Square> squares = rectStorage.GetShapes(out int squareCount);
            for (int i = 0; i < squareCount; i++)
            {
                Square square = squares[i];
                GL.Color(square.color);
                float2 center = square.center;
                float halfSize = square.size * 0.5f;
                float2 p0 = center + new float2(-halfSize, -halfSize);
                float2 p1 = center + new float2(-halfSize, +halfSize);
                float2 p2 = center + new float2(+halfSize, +halfSize);
                float2 p3 = center + new float2(+halfSize, -halfSize);
                GL_Quad(p0, p1, p2, p3);
            }
        }

        public static void DrawPolygons(ShapeStorage<Polygon> polygonStorage)
        {
            NativeArray<Polygon> polygons = polygonStorage.GetShapes(out int polygonCount);
            for (int i = 0; i < polygonCount; i++)
            {
                Polygon polygon = polygons[i];
                GL.Color(polygon.color);
                float step = BMath.TWO_PI / polygon.topology;
                float2 forward = new(polygon.radius, 0);
                float2 startPoint = polygon.center + forward;
                float2 prevPoint = startPoint;

                for (int j = 1; j < polygon.topology; j++)
                {
                    BMath.SinCos(step * j, out float sin, out float cos);
                    float2 newPoint = polygon.center + BMath.Rotate(forward, sin, cos);
                    GL_Line(prevPoint, newPoint);
                    prevPoint = newPoint;
                }
                
                GL_Line(prevPoint, startPoint);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GL_Quad(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            GL_Line(p0, p1);
            GL_Line(p1, p2);
            GL_Line(p2, p3);
            GL_Line(p3, p0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GL_Line(float2 start, float2 end)
        {
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
        }
    }
}