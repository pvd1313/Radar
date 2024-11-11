using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Input;
using Unity.Mathematics;
using UnityEngine;

using Time = UnityEngine.Time;

namespace Beast.Radar.Cinematic
{
    public class CameraController : Controller, IGameCycleListener
    {
        private readonly CameraConfig _config;
        private readonly ICameraTransform _transform;
        private readonly IScreen _screen;
        private readonly IInputDevice _input;

        private float2 _dragCameraPosition;

        public CameraController(
            CameraConfig config,
            ICameraTransform transform,
            IScreen screen,
            IInputDevice input)
        {
            _config = config;

            _transform = transform;
            _screen = screen;
            _input = input;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenBlocking(ActionID.TICK, _tick);
        }
        
        private void _tick(BlockingContext context)
        {
            if (!_transform.IsReady)
            {
                return;
            }

            _move();
            _zoom();
        }

        private void _move()
        {
            DragInput dragInput = _input.Drag;
            
            if (dragInput.status == EInputStatus.Starting)
            {
                _dragCameraPosition = _transform.Position;
            }

            if (dragInput.status != EInputStatus.Inactive)
            {
                float2 delta = _transform.ViewportToWorld(dragInput.startPosition) - 
                               _transform.ViewportToWorld(dragInput.position);

                _transform.SetPosition(_dragCameraPosition + delta);
            }
        }

        private void _zoom()
        {
            ZoomInput zoomInput = _input.Zoom;
            
            if (!zoomInput.isActive)
            {
                return;
            }

            float2 beforeZoom = _transform.ViewportToWorld(zoomInput.position);
            
            float factor = BMath.Lerp(1e-4f, 15f, _transform.Zoom01);
            float zoomValue = zoomInput.delta * factor * Time.smoothDeltaTime;
            _transform.AddZoom(zoomValue);
            
            float2 afterZoom = _transform.ViewportToWorld(zoomInput.position);
            _transform.AddPosition(beforeZoom - afterZoom, Space.World);
        }
    }
}