using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.UI;
using Unity.Mathematics;
using UnityEngine;
using UInput = UnityEngine.Input;

namespace Beast.Radar.Input
{
    public class TouchScreenDevice : Controller, IInputDevice, IGameCycleListener
    {
        public DragInput drag => _drag;
        
        private readonly IScreen _screen;
        private readonly IWindowManager _windows;

        private DragInput _drag;

        public TouchScreenDevice(IScreen screen, IWindowManager windows)
        {
            _screen = screen;
            _windows = windows;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
        }

        private void _tick(BlockingContext obj)
        {
            GetTouchInfo(0, out EInputStatus touchStatus0, out float2 touchPosition0);
            
            touchPosition0 /= _screen.Size;
            
            bool isMouseOverUI = _windows.IsOverUI(touchPosition0);

            if (_drag.status != EInputStatus.Inactive)
            {
                _drag.position = touchPosition0;
                _drag.status = touchStatus0;
                return;
            }

            if (isMouseOverUI)
            {
                _drag.position = float2.zero;
                _drag.status = EInputStatus.Inactive;
                return;
            }

            if (touchStatus0 == EInputStatus.Starting)
            {
                _drag.position = touchPosition0;
                _drag.startPosition = touchPosition0;
                _drag.status = EInputStatus.Starting;
            }
        }

        private void GetTouchInfo(int touchIndex, out EInputStatus status, out float2 position)
        {
            status = EInputStatus.Inactive;
            position = float2.zero;
            
            if (touchIndex >= UInput.touchCount)
            {
                return; 
            }

            Touch touch = UInput.GetTouch(touchIndex);
            position = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                status = EInputStatus.Starting;
                return;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                status = EInputStatus.Stopping;
                return;
            }

            status = EInputStatus.Working;
        }
    }
}