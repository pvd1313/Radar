using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public class TouchScreen : Gesture, IInputDevice
    {
        private const int MAX_TOUCH_COUNT = 16;

        public string FriendlyName => nameof(TouchScreen);

        private float _startZoomDistance;
        
        public UniTask Warmup(ConcurrentContext context)
        {
            _warmup(MAX_TOUCH_COUNT);
            
            _resetActiveAxisAndConsumer();
            
            return UniTask.CompletedTask;
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            _deactivateActiveAxis();
        }

        public PackedAxis GetActiveAxis()
        {
            return _packActiveAxis();
        }

        public void Tick(BlockingContext obj)
        {
            _driver.ReadTouchScreen(ref _rawTouches);

            try
            {
                _processRawTouches();

                RawTouchAxis touch0 = _rawTouches.Count > 0 ? _rawTouches[0] : RawTouchAxis.Disabled;
                RawTouchAxis touch1 = _rawTouches.Count > 1 ? _rawTouches[1] : RawTouchAxis.Disabled;
                
                _processTap(touch0);

                _processDrag(touch0, touch1);

                _processZoom(touch0, touch1);
            }
            catch (AxisMutatedException)
            {
                
            }
            
            BDebug.Track(this, nameof(_activeAxis), _activeAxis.ToString());
        }

        private void _processDrag(RawTouchAxis touch0, RawTouchAxis touch1)
        {
            if (_activeAxis == AxisType.DRAG)
            {
                // Mutation Drag -> Zoom
                if (_rawTouches.Count == 2 &&
                    _rawTouches.ContainsStartingTouch() &&
                    AxisType.ZOOM.Intersects(_consumer.axisMask))
                {
                    // Disable drag.
                    _drag.status = EAxisStatus.Disabled;
                    _consumer.UpdateAxis(new PackedAxis(_drag));

                    // Zoom will be updated in the next frame. No need to pass zero delta.
                    _zoom = _beginZoom(touch0.position, touch1.position);
                    _activeAxis = AxisType.ZOOM;

                    return;
                }

                _drag.position = touch0.position;
                _drag.status = touch0.status;
                _consumer.UpdateAxis(new PackedAxis(_drag));

                if (touch0.status == EAxisStatus.Disabled)
                {
                    _resetActiveAxisAndConsumer();
                    return;
                }
            }

            if (_activeAxis == AxisType.NONE &&
                _rawTouches.Count == 1 &&
                touch0.status == EAxisStatus.Starting)
            {
                _drag = DragAxis.StartingAt(touch0.position);

                PackedAxis packedDrag = new (_drag);

                if (_manager.TryFindConsumer(packedDrag, out _consumer))
                {
                    _activeAxis = AxisType.DRAG;
                    _consumer.UpdateAxis(packedDrag);
                }
            }
        }

        private void _processZoom(RawTouchAxis touch0, RawTouchAxis touch1)
        {
            if (_activeAxis == AxisType.ZOOM)
            {
                bool a = _rawTouches.Count == 1 && touch0.status != EAxisStatus.Disabled;
                bool b = _rawTouches.Count == 2 && _rawTouches.ContainsDisabledTouch();
                bool c = AxisType.DRAG.Intersects(_consumer.axisMask);
                
                // Mutation Zoom -> Drag
                if ((a || b) && c)
                {
                    // No need to notify about zoom stopped.

                    float2 pos = touch0.status == EAxisStatus.Disabled ? touch1.position : touch0.position;
                    
                    _drag = DragAxis.StartingAt(pos);
                    
                    _activeAxis = AxisType.DRAG;
                    
                    _consumer.UpdateAxis(new PackedAxis(_drag));
                    
                    throw AxisMutatedException.Default;
                }

                if (_rawTouches.Count != 2 || 
                    touch0.status == EAxisStatus.Disabled ||
                    touch1.status == EAxisStatus.Disabled)
                {
                    // No need to notify about zoom stopped.
                    
                    _resetActiveAxisAndConsumer();
                    
                    return;
                }

                _zoom = _continueZoom(touch0.position, touch1.position);
                
                _consumer.UpdateAxis(new PackedAxis(_zoom));
            }

            if (_activeAxis == AxisType.NONE &&
                _rawTouches.Count == 2 &&
                _rawTouches.ContainsStartingTouch())
            {
                _zoom = _beginZoom(touch0.position, touch1.position);
                
                if (_manager.TryFindConsumer(new PackedAxis(_zoom), out _consumer))
                {
                    // Zoom will be updated in the next frame.
                    // No need to pass zero delta.
                    
                    _activeAxis = AxisType.ZOOM;
                }
            }
        }

        private ZoomAxis _beginZoom(float2 touchPosition0, float2 touchPosition1)
        {
            _startZoomDistance = _screen.ViewportDistance(touchPosition0, touchPosition1);
            
            return _continueZoom(touchPosition0, touchPosition1);
        }

        private ZoomAxis _continueZoom(float2 touchPosition0, float2 touchPosition1)
        {
            float zoomDistance = BMath.Distance(touchPosition0, touchPosition1);
            
            

            ZoomAxis axis = default;
            axis.position = (touchPosition1 + touchPosition0) * 0.5f;
            axis.delta = (_startZoomDistance - zoomDistance);

            if (_config.touchScreen.useDiscreteZoom)
            {
                axis.delta *= _config.touchScreen.discreteZoomMultiplier;

                _startZoomDistance = zoomDistance;
            }
            else
            {
                axis.delta *= _config.mouse.zoomMultiplier;
            }

            return axis;
        }

        public void Dispose(BlockingContext context)
        {
            _rawTouches.Dispose();
        }
    }
}