using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public abstract class Gesture : Controller
    {
        protected readonly IInputDriver _driver;
        protected readonly IScreen _screen;
        protected readonly IInputManager _manager;
        protected readonly InputConfig _config;
        protected readonly IFrameTime _time;

        protected DragAxis _drag;
        protected ZoomAxis _zoom;
        protected NativeFixedList<RawTouchAxis> _rawTouches;
        
        private TapAxis _tap;
        private float2 _firstTapPosition;
        private float _tapTimer;
        
        protected ConsumerInfo _consumer;
        protected AxisType _activeAxis;

        protected Gesture(
            IInputDriver driver,
            IScreen screen,
            IInputManager manager, 
            IFrameTime time, 
            InputConfig config)
        {
            _driver = driver;
            _screen = screen;
            _manager = manager;
            _time = time;
            _config = config;
        }

        protected void _warmup(int maxTouchCount)
        {
            _rawTouches = new NativeFixedList<RawTouchAxis>(maxTouchCount, Allocator.Persistent);
            _drag = DragAxis.Inactive;
            _zoom = ZoomAxis.Inactive;
            
            _resetActiveAxisAndConsumer();
        }
        
        protected void _processRawTouches()
        {
            if (_activeAxis == AxisType.RAW_TOUCH)
            {
                _consumer.UpdateAxis(new PackedAxis(_rawTouches));
                
                if (!_rawTouches.ContainsWorkingOrStartingTouch())
                {
                    _resetActiveAxisAndConsumer();
                }

                return;
            }

            if (_activeAxis == AxisType.NONE &&
                _rawTouches.ContainsStartingTouch())
            {
                PackedAxis packedAxis = new (_rawTouches);

                if (_manager.TryFindConsumer(packedAxis, out _consumer))
                {
                    _activeAxis = AxisType.RAW_TOUCH;
                    _consumer.UpdateAxis(packedAxis);
                }
            }
        }

        protected void _processTap(RawTouchAxis touch0)
        {
            if (_activeAxis == AxisType.TAP)
            {
                _tapTimer += _time.DeltaTimeFloat;

                if (_rawTouches.Count > 1)
                {
                    // No need to notify consumer.
                    _resetActiveAxisAndConsumer();
                    return;
                }

                float fromPrevTapDistance = _screen.ViewportDistance(touch0.position, _tap.position);
                bool isTimeOver = _tapTimer >= _config.discardTapWhenHoldSeconds;
                bool isBigMotion = fromPrevTapDistance > _config.discardTapOnHoldMotionThreshold;
                
                // Hold timeout or motion threshold.
                // Mutation Tap -> Drag.
                if (touch0.status == EAxisStatus.Working && (isTimeOver || isBigMotion))
                {
                    _drag = DragAxis.StartingAt(touch0.position);
                    
                    PackedAxis packedDrag = new(_drag);

                    if (AxisType.DRAG.Intersects(_consumer.axisMask) ||
                        _manager.TryFindConsumer(packedDrag, out _consumer))
                    {
                        _activeAxis = AxisType.DRAG;
                        
                        _consumer.UpdateAxis(packedDrag);
                        
                        throw AxisMutatedException.Default;
                    } 
                }

                // Touch up once.
                if (_rawTouches.Count == 1 && 
                    touch0.status == EAxisStatus.Disabled)
                {
                    if (_tapTimer >= _config.discardTapWhenHoldSeconds)
                    {
                        _resetActiveAxisAndConsumer();
                        return;
                    }

                    _tap.tapCount++;
                    _tap.position = touch0.position;
                    _tapTimer = 0;
                    
                    _consumer.UpdateAxis(new PackedAxis(_tap));
                    
                    return;
                }

                // Trying discard combo by idle time.
                if (touch0.status == EAxisStatus.Disabled && 
                    _tapTimer >= _config.discardTapComboAfterSeconds)
                {
                    _resetActiveAxisAndConsumer();
                    return;
                }

                // Touch down once.
                if (touch0.status == EAxisStatus.Starting)
                {
                    // This is not perfect. Probably consumer should decide if it can consume taps.
                    // For example, this can be UI element, which has borders, and if new tap is outside the borders,
                    // the combo is discarded.
                    float fromFirstTapDistance = _screen.ViewportDistance(_firstTapPosition, touch0.position);
                    
                    if (_tapTimer >= _config.discardTapComboAfterSeconds ||
                        fromFirstTapDistance > _config.discardTapComboDistance)
                    {
                        _resetActiveAxisAndConsumer();
                        
                        // Will try to start combo again, code at the end.
                    }
                    else
                    {
                        _tapTimer = 0;
                        _tap.position = touch0.position;
                        
                        return;
                    }
                }
            }

            if (_activeAxis == AxisType.NONE &&
                _rawTouches.Count == 1 &&
                touch0.status == EAxisStatus.Starting)
            {
                TapAxis tapAxis = new (touch0.position, 0);

                if (_manager.TryFindConsumer(new PackedAxis(tapAxis), out _consumer))
                {
                    _tap = tapAxis;
                    _tapTimer = 0;
                    _activeAxis = AxisType.TAP;
                    _firstTapPosition = touch0.position;
                }
            }
        }

        protected void _deactivateActiveAxis()
        {
            if (_activeAxis == AxisType.RAW_TOUCH)
            {
                RawTouchAxis.DeactivateAll(_rawTouches);
                
                _consumer.UpdateAxis(new PackedAxis(_rawTouches));
            }
            else if (_activeAxis == AxisType.DRAG)
            {
                _drag.status = EAxisStatus.Disabled;
                
                _consumer.UpdateAxis(new PackedAxis(_drag));
            }
            else if (_activeAxis == AxisType.TAP)
            {
                // No need to notify consumer.
            }
            else if (_activeAxis == AxisType.ZOOM)
            {
                // No need to notify consumer.
            }

            _resetActiveAxisAndConsumer();
        }

        protected PackedAxis _packActiveAxis()
        {
            if (_activeAxis == AxisType.RAW_TOUCH)
            {
                return new PackedAxis(_rawTouches);
            }

            if (_activeAxis == AxisType.DRAG)
            {
                return new PackedAxis(_drag);
            }

            if (_activeAxis == AxisType.ZOOM)
            {
                return new PackedAxis(_zoom);
            }

            if (_activeAxis == AxisType.TAP)
            {
                return new PackedAxis(_tap);
            }
            
            return PackedAxis.None;
        }
        
        /// <para>_activeAxis = AxisType.NONE;</para>
        /// <para>_consumer = default;</para>
        protected void _resetActiveAxisAndConsumer()
        {
            _activeAxis = AxisType.NONE;
            _consumer = default;
        }
    }
}