using System.Runtime.CompilerServices;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public struct GizmosHandle
    {
        private readonly ushort _id;
        private readonly GizmosManager _gizmos;

        private int _cashVersion;
        private bool _enabled;
        private Color _color;
        
        internal GizmosHandle(ushort id, GizmosManager gizmos)
        {
            _id = id;
            _gizmos = gizmos;
            _cashVersion = -1;
            _enabled = false;
            _color = Color.clear;
        }

        public void DrawLine(float2 start, float2 end)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleLineDraw(new Line
            {
                start = start,
                end = end,
                color = _color
            });
        }

        public void DrawLine(float x0, float y0, float x1, float y1)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleLineDraw(new Line
            {
                start = new float2(x0, y0),
                end = new float2(x1, y1),
                color = _color
            });
        }

        public bool CheckEnabled()
        {
            UpdateCash();

            return _enabled;
        }

        public void DrawWireRect(float2 center, float2 size, float angleRad)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleWireRectDraw(new Rect
            {
                center = center,
                size = new half2((half) size.x, (half) size.y),
                color = _color,
                rotationRad = (half) angleRad
            });
        }

        public void DrawWireSquare(float2 center, float size)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleWireSquareDraw(new Square
            {
                center = center,
                size = (half) size,
                color = _color
            });
        }

        public void DrawScreenSquare(float2 center, float size)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleScreenSquareDraw(new Square
            {
                center = center,
                size = (half) size,
                color = _color
            });
        }

        public void DrawScreenDot(float2 center) => DrawScreenSquare(center, 6);

        public void DrawScreenCircle(float2 center, float radius)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }

            byte topology = (byte) System.Math.Min(255, System.Math.Max(32, 2 * BMath.PI * radius * 0.125f));
            _gizmos.ScheduleWirePolygonDraw(new Polygon
            {
                center = center,
                topology = topology,
                color = _color,
                radius = (half) radius
            });
        }

        public void DrawWirePolygon(float2 center, float radius, int topology)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleWirePolygonDraw(new Polygon
            {
                center = center,
                topology = (byte) topology,
                color = _color,
                radius = (half) radius
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCash()
        {
            int managerVersion = _gizmos.RenderVersion;
            if (managerVersion == _cashVersion)
            {
                return;
            }

            _cashVersion = managerVersion;
            _enabled = GizmosStateStorage.GetHandleEnabled(_id);
            _color = GizmosStateStorage.GetHandleColor(_id);
        }
    }
}