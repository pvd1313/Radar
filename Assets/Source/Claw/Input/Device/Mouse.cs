using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityInput = UnityEngine.Input;

namespace Beast.Claw.Input
{
    public class Mouse : Controller, IInputDevice
    {
        public string FriendlyName => nameof(Mouse);
        
        private readonly IScreen _screen;
        private readonly MouseConfig _config;
        private readonly IInputManager _manager;

        private DragAxis _drag;
        private ZoomAxis _zoom;
        private NativeFixedList<RawTouchAxis> _rawTouches;

        private RawTouchAxis _mouseAxis;
        
        private ConsumerInfo _consumer;
        private AxisType _activeAxis;
        
        public Mouse(IScreen screen, IInputManager manager, MouseConfig config)
        {
            _screen = screen;
            _config = config;
            _manager = manager;
        }

        public UniTask Warmup(ConcurrentContext context)
        {
            _rawTouches = new NativeFixedList<RawTouchAxis>(1, Allocator.Persistent);
            _rawTouches.Add(RawTouchAxis.Disabled);
            
            _setActiveAxisAndConsumerToDefault();
            
            return UniTask.CompletedTask;
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
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
        
        public PackedAxis GetActiveAxis()
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

        public void Tick(BlockingContext context)
        {
            float2 position = UnityInput.mousePosition.XY() /  _screen.Size;
            
            EAxisStatus status = _getMouseButtonStatus(0);
            
            _mouseAxis = new RawTouchAxis(position, status, 1);

            _rawTouches[0] = _mouseAxis;

            _processRawTouch();
            
            _processDrag();
            
            _processZoom();
            
            BDebug.Track(this, nameof(_activeAxis), _activeAxis.ToString());
        }

        private void _processRawTouch()
        {
            if (_activeAxis == AxisType.RAW_TOUCH)
            {
                _consumer.UpdateAxis(new PackedAxis(_rawTouches));

                if (_mouseAxis.status == EAxisStatus.Disabled)
                {
                    _setActiveAxisAndConsumerToDefault();
                    
                    return;
                }
            }

            if (_activeAxis == AxisType.NONE &&
                _mouseAxis.status == EAxisStatus.Starting)
            {
                PackedAxis packedAxis = new(_rawTouches);

                if (_manager.TryFindConsumer(packedAxis, out _consumer))
                {
                    _activeAxis = AxisType.RAW_TOUCH;
                    _consumer.UpdateAxis(packedAxis);
                }
            }
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
            
            float mouseScrollDelta = UnityInput.mouseScrollDelta.y;
            FloatBounds2 viewport = new (0, 1);

            if (mouseScrollDelta != 0 && 
                viewport.Contains(_mouseAxis.position))
            {
                _zoom.delta = mouseScrollDelta * _config.zoomMultiplier * -1f;
                _zoom.position = _mouseAxis.position;

                PackedAxis packedAxis = new (_zoom);

                if (_manager.TryFindConsumer(packedAxis, out ConsumerInfo consumer))
                {
                    consumer.UpdateAxis(packedAxis);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EAxisStatus _getMouseButtonStatus(int buttonID)
        {
            if (UnityInput.GetMouseButtonDown(buttonID))
            {
                return EAxisStatus.Starting;
            }

            if (UnityInput.GetMouseButton(buttonID))
            {
                return EAxisStatus.Working;
            }

            return EAxisStatus.Disabled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _setActiveAxisAndConsumerToDefault()
        {
            _activeAxis = AxisType.NONE;
            _consumer = default;
        }
        
        public void Dispose(BlockingContext context)
        {
            _rawTouches.SafeDispose();
        }
    }
}