using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UInput = UnityEngine.Input;

namespace Beast.Radar.Input
{
    public class TouchDevice : Controller, IInputDevice, IGameCycleListener
    {
        public DragInput Drag => _drag;
        public ZoomInput Zoom => _zoom;

        private readonly IScreen _screen;
        private readonly IWindowManager _windows;
        private readonly TouchInputConfig _config;

        private DragInput _drag;
        private ZoomInput _zoom;

        private float _zoomDistance;

        public TouchDevice(IScreen screen, IWindowManager windows, TouchInputConfig config)
        {
            _screen = screen;
            _windows = windows;
            _config = config;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            UInput.gyro.enabled = false;
            UInput.compass.enabled = false;
            UInput.compensateSensors = false;
            UInput.simulateMouseWithTouches = false;
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext obj)
        {
            GetTouchInfo(0, out EInputStatus touchStatus0, out float2 touchPosition0);
            GetTouchInfo(1, out EInputStatus touchStatus1, out float2 touchPosition1);
            touchPosition0 /= _screen.Size;
            touchPosition1 /= _screen.Size;

            if (_zoom.isActive)
            {
                if (touchStatus1 == EInputStatus.Inactive)
                {
                    _zoom.isActive = false;

                    // Switch zoom -> drag.
                    if (touchStatus0 != EInputStatus.Inactive)
                    {
                        _drag = _activateDrag(touchPosition0);
                        return;
                    }
                }
                else
                {
                    _zoom = _updateZoom(touchPosition0, touchPosition1);
                    
                    return;
                }
            }
            
            if (_drag.status != EInputStatus.Inactive)
            {
                // Switch drag -> zoom.
                if (touchStatus1 != EInputStatus.Inactive)
                {
                    _drag.status = EInputStatus.Inactive;
                    _zoomDistance = BMath.Distance(touchPosition0, touchPosition1);
                    _zoom = _updateZoom(touchPosition0, touchPosition1);
                    return;
                }
                
                _drag.position = touchPosition0;
                _drag.status = touchStatus0;
                return;
            }

            if (touchStatus0 != EInputStatus.Inactive && 
                touchStatus1 != EInputStatus.Inactive &&
                !_windows.IsOverUI(touchPosition0) &&
                !_windows.IsOverUI(touchPosition1))
            {
                _zoomDistance = BMath.Distance(touchPosition0, touchPosition1);
                _zoom = _updateZoom(touchPosition0, touchPosition1);
                return;
            }

            if (touchStatus0 != EInputStatus.Inactive && 
                !_windows.IsOverUI(touchPosition0))
            {
                _drag = _activateDrag(touchPosition0);
                return;
            }
        }

        private ZoomInput _updateZoom(float2 touchPosition0, float2 touchPosition1)
        {
            float zoomDistance = BMath.Distance(touchPosition0, touchPosition1);

            ZoomInput input = default;
            input.isActive = true;
            input.position = (touchPosition1 + touchPosition0) * 0.5f;
            input.delta = (_zoomDistance - zoomDistance);

            if (_config.useDiscreteZoom)
            {
                input.delta *= _config.discreteZoomMultiplier;

                _zoomDistance = zoomDistance;
            }
            else
            {
                input.delta *= _config.continuousZoomMultiplier;
            }

            return input;
        }

        private DragInput _activateDrag(float2 touchPosition)
        {
            return new DragInput
            {
                position = touchPosition,
                startPosition = touchPosition,
                status = EInputStatus.Starting
            };
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
                status = EInputStatus.Inactive;
                return;
            }

            status = EInputStatus.Working;
        }
    }
}