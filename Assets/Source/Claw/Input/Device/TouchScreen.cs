using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using UnityInput = UnityEngine.Input;

namespace Beast.Claw.Input
{
    public class TouchScreen : Controller, IInputDevice
    {
        private const int MAX_TOUCH_COUNT = 16;

        public string FriendlyName => nameof(TouchScreen);

        private readonly IScreen _screen;
        private readonly IInputManager _manager;
        private readonly TouchScreenConfig _config;

        private DragAxis _drag;
        private ZoomAxis _zoom;
        private NativeFixedList<RawTouchAxis> _rawTouches;

        private ConsumerInfo _consumer;
        private EInputAxis _activeAxis;

        private float _startZoomDistance;
        private int _touchCount;

        public TouchScreen(IScreen screen, IInputManager manager, TouchScreenConfig config)
        {
            _screen = screen;
            _manager = manager;
            _config = config;
        }

        public UniTask Warmup(ConcurrentContext context)
        {
            _rawTouches = new NativeFixedList<RawTouchAxis>(MAX_TOUCH_COUNT, Allocator.Persistent);

            return UniTask.CompletedTask;
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            switch (_activeAxis)
            {
                case EInputAxis.RawTouch:
                    RawTouchAxis.DeactivateAll(_rawTouches);
                    _consumer.UpdateAxis(new PackedAxis(_rawTouches));
                    break;
                
                case EInputAxis.Drag:
                    _drag.status = EAxisStatus.Disabled;
                    _consumer.UpdateAxis(new PackedAxis(_drag));
                    break;
                
                case EInputAxis.Zoom:
                    // No need to reset zoom.
                    break;
            }

            _setActiveAxisAndConsumerToDefault();
        }

        public PackedAxis GetActiveAxis()
        {
            return _activeAxis switch
            {
                EInputAxis.RawTouch => new PackedAxis(_rawTouches),
                EInputAxis.Drag => new PackedAxis(_drag),
                EInputAxis.Zoom => new PackedAxis(_zoom),
                _ => PackedAxis.None
            };
        }

        public void Tick(BlockingContext obj)
        {
            _fetchRawTouches();

            _processRawTouch();

            RawTouchAxis touch0 = _touchCount > 0 ? _rawTouches[0] : RawTouchAxis.Disabled;
            RawTouchAxis touch1 = _touchCount > 1 ? _rawTouches[1] : RawTouchAxis.Disabled;

            _processDrag(touch0, touch1);

            _processZoom(touch0, touch1);
            
            BDebug.Track(this, nameof(_activeAxis), _activeAxis.ToString());
        }

        private void _processRawTouch()
        {
            if (_activeAxis == EInputAxis.RawTouch)
            {
                _consumer.UpdateAxis(new PackedAxis(_rawTouches));
                
                if (!_rawTouches.ContainsWorkingOrStartingTouch())
                {
                    _setActiveAxisAndConsumerToDefault();
                }

                return;
            }

            if (_activeAxis == EInputAxis.None &&
                _rawTouches.ContainsStartingTouch())
            {
                PackedAxis packedAxis = new (_rawTouches);

                if (_manager.TryFindConsumer(packedAxis, out _consumer))
                {
                    _activeAxis = EInputAxis.RawTouch;
                    _consumer.UpdateAxis(packedAxis);
                }
            }
        }

        private void _processDrag(RawTouchAxis touch0, RawTouchAxis touch1)
        {
            if (_activeAxis == EInputAxis.Drag)
            {
                // Switch Drag -> Zoom
                if (_touchCount == 2 &&
                    _rawTouches.ContainsStartingTouch() &&
                    _consumer.axisMask.Intersects((int)EInputAxis.Zoom))
                {
                    // Disable drag.
                    _drag.status = EAxisStatus.Disabled;
                    _consumer.UpdateAxis(new PackedAxis(_drag));

                    // Zoom will be updated in the next frame. No need to pass zero delta.
                    _zoom = _beginZoom(touch0.position, touch1.position);
                    _activeAxis = EInputAxis.Zoom;

                    return;
                }

                _drag.position = touch0.position;
                _drag.status = touch0.status;
                _consumer.UpdateAxis(new PackedAxis(_drag));

                if (touch0.status == EAxisStatus.Disabled)
                {
                    _setActiveAxisAndConsumerToDefault();
                    return;
                }
            }

            if (_activeAxis == EInputAxis.None &&
                _touchCount == 1 &&
                touch0.status == EAxisStatus.Starting)
            {
                _drag = DragAxis.StartingAt(touch0.position);

                PackedAxis packedDrag = new (_drag);

                if (_manager.TryFindConsumer(packedDrag, out _consumer))
                {
                    _activeAxis = EInputAxis.Drag;
                    _consumer.UpdateAxis(packedDrag);
                    return;
                }

                _drag = DragAxis.Inactive;
            }
        }

        private void _processZoom(RawTouchAxis touch0, RawTouchAxis touch1)
        {
            if (_activeAxis == EInputAxis.Zoom)
            {
                bool a = _touchCount == 1 && touch0.status != EAxisStatus.Disabled;
                bool b = _touchCount == 2 && _rawTouches.ContainsDisabledTouch();
                bool c = _consumer.axisMask.Intersects((int)EInputAxis.Drag);
                
                // Switch Zoom -> Drag
                if ((a || b) && c)
                {
                    // No need to notify about zoom stopped.

                    float2 pos = touch0.status == EAxisStatus.Disabled ? touch1.position : touch0.position;
                    
                    _drag = DragAxis.StartingAt(pos);
                    
                    _activeAxis = EInputAxis.Drag;
                    
                    _consumer.UpdateAxis(new PackedAxis(_drag));
                    
                    return;
                }

                if (_touchCount != 2 || 
                    touch0.status == EAxisStatus.Disabled ||
                    touch1.status == EAxisStatus.Disabled)
                {
                    // No need to notify about zoom stopped.
                    
                    _setActiveAxisAndConsumerToDefault();
                    
                    return;
                }

                _zoom = _continueZoom(touch0.position, touch1.position);
                
                _consumer.UpdateAxis(new PackedAxis(_zoom));
            }

            if (_activeAxis == EInputAxis.None &&
                _touchCount == 2 &&
                _rawTouches.ContainsStartingTouch())
            {
                _zoom = _beginZoom(touch0.position, touch1.position);
                
                if (_manager.TryFindConsumer(new PackedAxis(_zoom), out _consumer))
                {
                    // Zoom will be updated in the next frame. No need to pass zero delta.
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
            _startZoomDistance = BMath.Distance(touchPosition0, touchPosition1);
            
            return _continueZoom(touchPosition0, touchPosition1);
        }

        private ZoomAxis _continueZoom(float2 touchPosition0, float2 touchPosition1)
        {
            float zoomDistance = BMath.Distance(touchPosition0, touchPosition1);

            ZoomAxis axis = default;
            axis.position = (touchPosition1 + touchPosition0) * 0.5f;
            axis.delta = (_startZoomDistance - zoomDistance);

            if (_config.useDiscreteZoom)
            {
                axis.delta *= _config.discreteZoomMultiplier;

                _startZoomDistance = zoomDistance;
            }
            else
            {
                axis.delta *= _config.continuousZoomMultiplier;
            }

            return axis;
        }

        private void _fetchRawTouches()
        {
            _touchCount = UnityInput.touchCount;
            _rawTouches.ResetCount();

            for (int i = 0; i < _touchCount && i < MAX_TOUCH_COUNT; i++)
            {
                Touch touch = UnityInput.GetTouch(i);

                EAxisStatus status = EAxisStatus.Disabled;

                if (touch.phase == TouchPhase.Began)
                {
                    status = EAxisStatus.Starting;
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    status = EAxisStatus.Working;
                }
                
                _rawTouches.Add(new RawTouchAxis
                {
                    fingerId = touch.fingerId,
                    status = status,
                    position = (float2)touch.position / _screen.Size
                });
            }
        }

        private void _setActiveAxisAndConsumerToDefault()
        {
            _activeAxis = EInputAxis.None;
            _consumer = default;
        }
        
        public void Dispose(BlockingContext context)
        {
            _rawTouches.SafeDispose();
        }
    }
}