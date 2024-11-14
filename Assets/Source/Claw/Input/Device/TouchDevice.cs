using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Beast.Claw.Input
{
    public class TouchDevice : Controller, IInputDevice, IGameCycleListener
    {
        private const int MAX_TOUCH_COUNT = 16;
        
        public DragAxis Drag => _drag;
        public ZoomAxis Zoom => _zoom;
        public NativeSlice<RawTouchAxis> RawTouch => _rawTouch;

        private readonly IScreen _screen;
        private readonly IInputManager _manager;
        private readonly TouchInputConfig _config;

        private DragAxis _drag;
        private ZoomAxis _zoom;
        private NativeFixedList<RawTouchAxis> _rawTouch;

        private ConsumerInfo _consumer;
        private EInputAxis _activeAxis;

        private float _zoomDistance;
        private int _touchCount;

        public TouchDevice(IScreen screen, IInputManager manager, TouchInputConfig config)
        {
            _screen = screen;
            _manager = manager;
            _config = config;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
            handle.ListenBlocking(ActionID.TICK, _tick);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            UnityInput.gyro.enabled = false;
            UnityInput.compass.enabled = false;
            UnityInput.compensateSensors = false;
            UnityInput.simulateMouseWithTouches = false;

            _rawTouch = new NativeFixedList<RawTouchAxis>(MAX_TOUCH_COUNT, Allocator.Persistent);

            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext obj)
        {
            _fetchTouchAxis();

            _processRawTouch();

            RawTouchAxis touch0 = _touchCount > 0 ? _rawTouch[0] : RawTouchAxis.Inactive;
            RawTouchAxis touch1 = _touchCount > 1 ? _rawTouch[1] : RawTouchAxis.Inactive;

            _processDrag(touch0, touch1);

            _processZoom(touch0, touch1);
            
            BDebug.Track(this, nameof(_activeAxis), _activeAxis.ToString());
        }

        private void _processRawTouch()
        {
            if (_activeAxis == EInputAxis.RawTouch)
            {
                if (!RawTouchAxis.ContainsActiveAxis(_rawTouch))
                {
                    _consumer = default;
                    _activeAxis = EInputAxis.None;
                }

                return;
            }

            if (_activeAxis == EInputAxis.None &&
                RawTouchAxis.ContainsAxisWithStatus(_rawTouch, EAxisStatus.Starting) &&
                _manager.TryFindConsumer(this, EInputAxis.RawTouch, out _consumer))
            {
                _activeAxis = EInputAxis.RawTouch;
                _consumer.consumer.UpdateInput(this, _activeAxis);
            }
        }

        private void _processDrag(RawTouchAxis touch0, RawTouchAxis touch1)
        {
            if (_activeAxis == EInputAxis.Drag)
            {
                // Switch drag -> zoom
                if (_touchCount == 2 &&
                    RawTouchAxis.ContainsAxisWithStatus(_rawTouch, EAxisStatus.Starting) &&
                    _consumer.axisMask.Intersects((int)EInputAxis.Zoom))
                {
                    _drag = DragAxis.Inactive;
                    _consumer.consumer.UpdateInput(this, EInputAxis.Drag);

                    _zoom = _beginZoom(touch0.position, touch1.position);
                    _activeAxis = EInputAxis.Zoom;

                    return;
                }

                _drag.position = touch0.position;
                _drag.status = touch0.status;
                _consumer.consumer.UpdateInput(this, EInputAxis.Drag);

                if (touch0.status == EAxisStatus.Inactive)
                {
                    _consumer = default;
                    _activeAxis = EInputAxis.None;
                    return;
                }
            }

            if (_activeAxis == EInputAxis.None &&
                _touchCount == 1 &&
                touch0.status == EAxisStatus.Starting)
            {
                _drag = DragAxis.CreateStarting(touch0.position);

                if (_manager.TryFindConsumer(this, EInputAxis.Drag, out _consumer))
                {
                    _activeAxis = EInputAxis.Drag;
                    _consumer.consumer.UpdateInput(this, EInputAxis.Drag);
                    return;
                }

                _drag = DragAxis.Inactive;
            }
        }

        private void _processZoom(RawTouchAxis touch0, RawTouchAxis touch1)
        {
            if (_activeAxis == EInputAxis.Zoom)
            {
                // Switch zoom -> drag
                if (touch1.status == EAxisStatus.Inactive &&
                    _consumer.axisMask.Intersects((int)EInputAxis.Drag))
                {
                    _drag = DragAxis.CreateStarting(touch0.position);
                    _activeAxis = EInputAxis.Drag;
                    _consumer.consumer.UpdateInput(this, EInputAxis.Drag);
                    return;
                }

                if (touch1.status == EAxisStatus.Inactive)
                {
                    _activeAxis = EInputAxis.None;
                    _consumer = default;
                    return;
                }

                _zoom = _continueZoom(touch0.position, touch1.position);
                _consumer.consumer.UpdateInput(this, EInputAxis.Zoom);
            }

            if (_activeAxis == EInputAxis.None &&
                _touchCount == 2 &&
                touch1.status == EAxisStatus.Starting)
            {
                _zoom = _beginZoom(touch0.position, touch1.position);

                if (_manager.TryFindConsumer(this, EInputAxis.Zoom, out _consumer))
                {
                    _activeAxis = EInputAxis.Zoom;
                }
                else
                {
                    _zoom = default;
                }
            }
        }

        private ZoomAxis _beginZoom(float2 touchPosition0, float2 touchPosition1)
        {
            _zoomDistance = BMath.Distance(touchPosition0, touchPosition1);
            
            return _continueZoom(touchPosition0, touchPosition1);
        }

        private ZoomAxis _continueZoom(float2 touchPosition0, float2 touchPosition1)
        {
            float zoomDistance = BMath.Distance(touchPosition0, touchPosition1);

            ZoomAxis axis = default;
            axis.position = (touchPosition1 + touchPosition0) * 0.5f;
            axis.delta = (_zoomDistance - zoomDistance);

            if (_config.useDiscreteZoom)
            {
                axis.delta *= _config.discreteZoomMultiplier;

                _zoomDistance = zoomDistance;
            }
            else
            {
                axis.delta *= _config.continuousZoomMultiplier;
            }

            return axis;
        }

        private void _fetchTouchAxis()
        {
            _touchCount = UnityInput.touchCount;
            _rawTouch.Clear();

            for (int i = 0; i < _touchCount && i < MAX_TOUCH_COUNT; i++)
            {
                Touch touch = UnityInput.GetTouch(i);

                EAxisStatus status = EAxisStatus.Inactive;

                if (touch.phase == TouchPhase.Began)
                {
                    status = EAxisStatus.Starting;
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    status = EAxisStatus.Working;
                }
                
                _rawTouch.Add(new RawTouchAxis
                {
                    fingerId = touch.fingerId,
                    status = status,
                    position = (float2)touch.position / _screen.Size
                });
            }
        }
        
        private void _dispose(BlockingContext obj)
        {
            _rawTouch.Dispose();
        }
    }
}