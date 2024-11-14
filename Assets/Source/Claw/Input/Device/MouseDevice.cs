using System.Runtime.CompilerServices;
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
    public class MouseDevice : Controller, IInputDevice, IGameCycleListener
    {
        public DragAxis Drag => _drag;
        public ZoomAxis Zoom => _zoom;
        public NativeSlice<RawTouchAxis> RawTouch => _rawTouch;
        
        private readonly IScreen _screen;
        private readonly MouseInputConfig _config;
        private readonly IInputManager _manager;

        private DragAxis _drag;
        private ZoomAxis _zoom;
        private NativeFixedList<RawTouchAxis> _rawTouch;

        private RawTouchAxis _mouseAxis;
        
        private ConsumerInfo _consumer;
        private EInputAxis _activeAxis;
        
        public MouseDevice(IScreen screen, MouseInputConfig config, IInputManager manager)
        {
            _screen = screen;
            _config = config;
            _manager = manager;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _rawTouch = new NativeFixedList<RawTouchAxis>(1, Allocator.Persistent);
            
            return UniTask.CompletedTask;
        }

        private void _tick(BlockingContext context)
        {
            float2 position = _getMouseScreenPosition() / _screen.Size;
            EAxisStatus status = _getMouseButtonStatus(0);
            _mouseAxis = new RawTouchAxis(position, status, 1);

            _rawTouch.Clear();
            _rawTouch.Add(_mouseAxis);

            _processRawTouch();
            
            _processDrag();
            
            _processZoom();
            
            BDebug.Track(this, nameof(_activeAxis), _activeAxis.ToString());
        }

        private void _processRawTouch()
        {
            if (_activeAxis == EInputAxis.RawTouch)
            {
                _consumer.consumer.UpdateInput(this, EInputAxis.RawTouch);

                if (_mouseAxis.status == EAxisStatus.Inactive)
                {
                    _consumer = default;
                    _activeAxis = EInputAxis.None;
                    
                    return;
                }
            }

            if (_activeAxis == EInputAxis.None && 
                _mouseAxis.status == EAxisStatus.Starting &&
                _manager.TryFindConsumer(this, EInputAxis.RawTouch, out _consumer))
            {
                _activeAxis = EInputAxis.RawTouch;
                _consumer.consumer.UpdateInput(this, _activeAxis);
            }
        }
        
        private void _processDrag()
        {
            if (_activeAxis == EInputAxis.Drag)
            {
                _drag.position = _mouseAxis.position;
                _drag.status = _mouseAxis.status;
                _consumer.consumer.UpdateInput(this, EInputAxis.Drag);

                if (_mouseAxis.status == EAxisStatus.Inactive)
                {
                    _consumer = default;
                    _activeAxis = EInputAxis.None;
                }
                
                return;
            }

            if (_activeAxis == EInputAxis.None &&
                _mouseAxis.status == EAxisStatus.Starting)
            {
                _drag.startPosition = _mouseAxis.position;
                _drag.position = _mouseAxis.position;
                _drag.status = EAxisStatus.Starting;

                if (_manager.TryFindConsumer(this, EInputAxis.Drag, out _consumer))
                {
                    _activeAxis = EInputAxis.Drag;
                    _consumer.consumer.UpdateInput(this, EInputAxis.Drag);
                }
                else
                {
                    _drag = DragAxis.Inactive;   
                }
            }
        }

        private void _processZoom()
        {
            if (_activeAxis != EInputAxis.None)
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

                if (_manager.TryFindConsumer(this, EInputAxis.Zoom, out ConsumerInfo consumer))
                {
                    consumer.consumer.UpdateInput(this, EInputAxis.Zoom);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 _getMouseScreenPosition()
        {
            Vector3 position = UnityInput.mousePosition;

            return new float2(position.x, position.y);
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

            return EAxisStatus.Inactive;
        }
        
        private void _dispose(BlockingContext obj)
        {
            _rawTouch.Dispose();
        }
    }
}