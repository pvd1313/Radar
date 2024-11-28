using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Input
{
    public class Mouse : Gesture, IInputDevice
    {
        public string FriendlyName => nameof(Mouse);
        
        private RawTouchAxis _mouseAxis;
        private RawTouchAxis _mouseAxisPrev;

        public UniTask Warmup(ConcurrentContext context)
        {
            _warmup(1);
            
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

        public void Tick(BlockingContext context)
        {
            _mouseAxis = _driver.ReadMousePointer();
            
            _rawTouches.Clear();

            // We need to remove rawTouch when it is not active for two frames.
            // This will mimic TouchScreen behaviour.
            if (_mouseAxis.status != EAxisStatus.Disabled ||
                _mouseAxisPrev.status != EAxisStatus.Disabled)
            {
                _rawTouches.Add(_mouseAxis);
            }

            try
            {
                _processRawTouches();

                _processTap(_mouseAxis);
            
                _processDrag();
            
                _processZoom();
            }
            catch (AxisMutatedException)
            {
                   
            }

            _mouseAxisPrev = _mouseAxis;
            
            BDebug.Track(this, nameof(_activeAxis), _activeAxis.ToString());
        }
        
        private void _processDrag()
        {
            if (_activeAxis == AxisType.DRAG)
            {
                _drag.position = _mouseAxis.position;
                _drag.status = _mouseAxis.status;
                _consumer.UpdateAxis(new PackedAxis(_drag));

                if (_mouseAxis.status == EAxisStatus.Disabled)
                {
                    _resetActiveAxisAndConsumer();
                }
                
                return;
            }

            if (_activeAxis == AxisType.NONE &&
                _mouseAxis.status == EAxisStatus.Starting)
            {
                DragAxis dragAxis = DragAxis.StartingAt(_mouseAxis.position);
                PackedAxis packedAxis = new (dragAxis);

                if (_manager.TryFindConsumer(packedAxis, out _consumer))
                {
                    _activeAxis = AxisType.DRAG;
                    _drag = dragAxis;
                    _consumer.UpdateAxis(packedAxis);
                }
            }
        }

        private void _processZoom()
        {
            if (_activeAxis != AxisType.NONE)
            {
                return;
            }

            float mouseScrollDelta = _driver.ReadMouseWheel();
            Bounds2D viewport = new (0, 1);

            if (mouseScrollDelta != 0 && 
                viewport.Contains(_mouseAxis.position))
            {
                _zoom.delta = mouseScrollDelta * _config.mouse.zoomMultiplier * -1f;
                _zoom.position = _mouseAxis.position;

                PackedAxis packedAxis = new (_zoom);

                if (_manager.TryFindConsumer(packedAxis, out ConsumerInfo consumer))
                {
                    consumer.UpdateAxis(packedAxis);
                }
            }
        }
        
        public void Dispose(BlockingContext context)
        {
            _rawTouches.Dispose();
        }
    }
}