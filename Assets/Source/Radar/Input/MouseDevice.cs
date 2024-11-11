using System.Runtime.CompilerServices;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.UI;
using Unity.Mathematics;
using UnityEngine;

using UInput = UnityEngine.Input;

namespace Beast.Radar.Input
{
    public class MouseDevice : Controller, IInputDevice, IGameCycleListener
    {
        public DragInput drag => _drag;
        
        private readonly IWindowManager _windows;
        private readonly IScreen _screen;

        private DragInput _drag;
        
        public MouseDevice(IWindowManager windows, IScreen screen)
        {
            _windows = windows;
            _screen = screen;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
        }
        private void _tick(BlockingContext context)
        {
            EInputStatus mouseButtonStatus = GetMouseButtonStatus(0);
            float2 mousePosition = GetMouseScreenPosition() / _screen.Size;
            
            if (_drag.status != EInputStatus.Inactive)
            {
                _drag.position = mousePosition;
                _drag.status = mouseButtonStatus;
                return;
            }

            if (mouseButtonStatus != EInputStatus.Inactive && _windows.IsOverUI(mousePosition))
            {
                _drag.status = EInputStatus.Inactive;
                return;
            }

            if (mouseButtonStatus == EInputStatus.Starting)
            {
                _drag.startPosition = mousePosition;
                _drag.position = mousePosition;
                _drag.status = EInputStatus.Starting;
            }
        }

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

            if (UInput.GetMouseButtonUp(buttonID))
            {
                return EInputStatus.Stopping;
            }

            return EInputStatus.Inactive;
        }
    }
}