using Beast.Claw.Collections;
using Beast.Claw.Common;
using Unity.Collections;

namespace Beast.Claw.Input
{
    public abstract class GestureDevice : Controller
    {
        protected readonly IInputManager _manager;
        
        protected DragAxis _drag;
        protected ZoomAxis _zoom;
        protected NativeFixedList<RawTouchAxis> _rawTouches;

        protected ConsumerInfo _consumer;
        protected AxisType _activeAxis;

        protected GestureDevice(IInputManager manager)
        {
            _manager = manager;
        }

        protected void _warmup(int maxTouchCount)
        {
            _rawTouches = new NativeFixedList<RawTouchAxis>(maxTouchCount, Allocator.Persistent);
            _drag = DragAxis.Inactive;
            _zoom = ZoomAxis.Inactive;
            
            _setActiveAxisAndConsumerToDefault();
        }
        
        protected void _processRawTouches()
        {
            if (_activeAxis == AxisType.RAW_TOUCH)
            {
                _consumer.UpdateAxis(new PackedAxis(_rawTouches));
                
                if (!_rawTouches.ContainsWorkingOrStartingTouch())
                {
                    _setActiveAxisAndConsumerToDefault();
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

        protected void _setActiveAxisAndConsumerToDefault()
        {
            _activeAxis = AxisType.NONE;
            _consumer = default;
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
            else if (_activeAxis == AxisType.ZOOM)
            {
                // No need to reset zoom.
            }

            _setActiveAxisAndConsumerToDefault();
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
            
            return PackedAxis.None;
        }
    }
}