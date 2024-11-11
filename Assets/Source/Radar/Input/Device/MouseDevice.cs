using System.Runtime.CompilerServices;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.UI;
using Unity.Mathematics;
using UnityEngine;

using UInput = UnityEngine.Input;

namespace Beast.Radar.Input
{
    public class MouseDevice : Controller, IInputDevice, IGameCycleListener
    {
        public DragInput Drag => _drag;
        public ZoomInput Zoom => _zoom;
        
        private readonly IWindowManager _windows;
        private readonly IScreen _screen;
        private readonly MouseInputConfig _config;

        private DragInput _drag;
        private ZoomInput _zoom;
        private float2 _mousePosition;
        
        public MouseDevice(IWindowManager windows, IScreen screen, MouseInputConfig config)
        {
            _windows = windows;
            _screen = screen;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
        }
        private void _tick(BlockingContext context)
        {
            _mousePosition = GetMouseScreenPosition() / _screen.Size;
            
            _processDrag();
            _processZoom();
        }

        private void _processZoom()
        {
            float mouseScrollDelta = UInput.mouseScrollDelta.y;
            FloatBounds2 viewport = new (0, 1);

            if (mouseScrollDelta != 0 && 
                viewport.Contains(_mousePosition) && 
                !_windows.IsOverUI(_mousePosition) && 
                _drag.status == EInputStatus.Inactive)
            {
                _zoom.isActive = true;
                _zoom.delta = mouseScrollDelta * _config.zoomMultiplier * -1f;
                _zoom.position = _mousePosition;
            }
            else
            {
                _zoom.isActive = false;
            }
        }

        private void _processDrag()
        {
            EInputStatus mouseButtonStatus = GetMouseButtonStatus(0);
            
            if (_drag.status != EInputStatus.Inactive)
            {
                _drag.position = _mousePosition;
                _drag.status = mouseButtonStatus;
                return;
            }

            if (mouseButtonStatus == EInputStatus.Starting && _windows.IsOverUI(_mousePosition))
            {
                _drag.status = EInputStatus.Inactive;
                return;
            }

            if (mouseButtonStatus == EInputStatus.Starting)
            {
                _drag.startPosition = _mousePosition;
                _drag.position = _mousePosition;
                _drag.status = EInputStatus.Starting;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 GetMouseScreenPosition()
        {
            Vector3 position = UInput.mousePosition;

            return new float2(position.x, position.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EInputStatus GetMouseButtonStatus(int buttonID)
        {
            if (UInput.GetMouseButtonDown(buttonID))
            {
                return EInputStatus.Starting;
            }

            if (UInput.GetMouseButton(buttonID))
            {
                return EInputStatus.Working;
            }

            return EInputStatus.Inactive;
        }
    }
}