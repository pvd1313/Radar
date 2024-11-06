using System.Diagnostics;
using System.Runtime.CompilerServices;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public struct GizmosHandle
    {
        private readonly ushort _id;
        private readonly GizmosRender _gizmos;

        private int _cashVersion;
        private bool _enabled;
        private half4 _color;
        
        internal GizmosHandle(ushort id, GizmosRender gizmos)
        {
            _id = id;
            _gizmos = gizmos;
            _cashVersion = -1;
            _enabled = false;
            _color = Color.clear.AsHalf4();
        }

        public bool IsEnabled()
        {
            _updateCash();

            return _enabled;
        }

        public void DrawPoint(float2 position)
        {
            _updateCash();

            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleQuad(new GizmosQuad
            {
                center = position,
                color = _color,
                size = (half) 3f
            });
        }
        
        public void DrawPoint(float2 position, float size)
        {
            _updateCash();

            if (!_enabled)
            {
                return;
            }
            
            _gizmos.ScheduleQuad(new GizmosQuad
            {
                center = position,
                color = _color,
                size = (half) size
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _updateCash()
        {
            int managerVersion = _gizmos.RenderVersion;
            if (managerVersion == _cashVersion)
            {
                return;
            }

            _cashVersion = managerVersion;
            _enabled = GizmosStateStorage.GetHandleEnabled(_id);
            _color = GizmosStateStorage.GetHandleColor(_id).AsHalf4();
        }
    }
}