using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Input
{
    public class Mouse : GestureDevice, IInputDevice
    {
        public string FriendlyName => nameof(Mouse);
        
        private readonly IInputDriver _driver;
        private readonly InputConfig _config;

        private RawTouchAxis _mouseAxis;
        
        public Mouse(
            IInputDriver driver, 
            IInputManager manager, 
            InputConfig config) : base(manager)
        {
            _driver = driver;
            _config = config;
        }

        public UniTask Warmup(ConcurrentContext context)
        {
            _warmup(1);
            
            _rawTouches.Add(RawTouchAxis.Disabled);
            
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

            _rawTouches[0] = _mouseAxis;

            _processRawTouches();
            
            _processDrag();
            
            _processZoom();
            
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
                    _setActiveAxisAndConsumerToDefault();
                }
                
                return;
            }

            if (_activeAxis == AxisType.NONE &&
                _mouseAxis.status == EAxisStatus.Starting)
            {
                _drag = DragAxis.StartingAt(_mouseAxis.position);
                
                PackedAxis packedAxis = new (_drag);

                if (_manager.TryFindConsumer(packedAxis, out _consumer))
                {
                    _activeAxis = AxisType.DRAG;
                    _consumer.UpdateAxis(packedAxis);
                }
                else
                {
                    _drag = DragAxis.Inactive;   
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
            FloatBounds2 viewport = new (0, 1);

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
            _rawTouches.SafeDispose();
        }
    }
}