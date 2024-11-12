using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Input;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : Controller, IGameCycleListener
    {
        private readonly ICameraTransform _transform;
        private readonly IInputDevice _input;

        private float2 _dragCameraPosition;

        public CameraController(ICameraTransform transform, IInputDevice input)
        {
            _transform = transform;
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
            float zoomValue = zoomInput.delta * factor * (1f / 60f);
            _transform.AddZoom(zoomValue);
            
            float2 afterZoom = _transform.ViewportToWorld(zoomInput.position);
            _transform.AddPosition(beforeZoom - afterZoom, Space.World);
        }
    }
}