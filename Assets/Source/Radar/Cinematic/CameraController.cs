using Beast.Claw.Common;
using Beast.Claw.Debug;
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
        private readonly CameraConfig _cameraConfig;

        private float2 _dragCameraPosition;

        public CameraController(ICameraTransform transform, IInputDevice input, CameraConfig cameraConfig)
        {
            _transform = transform;
            _input = input;
            _cameraConfig = cameraConfig;
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

            float smoothValue = BMath.Lerp(
                _transform.Zoom01, 
                _transform.Zoom01 + zoomValue, 
                _cameraConfig.zoomSmoothSpeed) - _transform.Zoom01;
            
            _transform.AddZoom(smoothValue);
            
            float2 afterZoom = _transform.ViewportToWorld(zoomInput.position);
            _transform.AddPosition(beforeZoom - afterZoom, Space.World);
        }
    }
}