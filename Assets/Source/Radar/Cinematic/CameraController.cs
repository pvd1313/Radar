using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.Input;
using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public class CameraController : Controller, IGameCycleListener
    {
        private readonly CameraConfig _cameraConfig;
        private readonly ICameraTransform _transform;
        private readonly IScreen _screen;
        private readonly IInputDevice _input;

        private float2 _dragCameraPosition;

        public CameraController(
            CameraConfig cameraConfig,
            ICameraTransform transform,
            IScreen screen,
            IInputDevice input)
        {
            _cameraConfig = cameraConfig;

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
            if (_input.drag.status == EInputStatus.Starting)
            {
                _dragCameraPosition = _transform.Position;
            }

            if (_input.drag.status != EInputStatus.Inactive)
            {
                float2 delta = _transform.ViewportToWorld(_input.drag.startPosition) - 
                               _transform.ViewportToWorld(_input.drag.position);

                _transform.SetPosition(_dragCameraPosition + delta);
            }
        }

        private void _zoom()
        {
            // TODO: #R-93 Restore.
            // if (_inputSystem.GameInput.Scroll.State == EButtonState.Default)
            // {
            //     return;
            // }
            //
            // float2 screenPoint = _inputSystem.GameInput.ScreenPoint.Value;
            // float2 worldPointBeforeZoom = _transform.ScreenToWorld(screenPoint);
            //
            // float distanceFactor = BMath.Lerp(0.0001f, 15f, _transform.Zoom01);
            // float zoomValue = _inputSystem.GameInput.Scroll.Value * distanceFactor * _motion.zoomSpeed 
            //                   * Time.smoothDeltaTime;
            // _transform.AddZoom(-zoomValue);
            //
            // float2 worldPointAfterZoom = _transform.ScreenToWorld(screenPoint);
            // float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;
            //
            // _transform.AddPosition(pointDelta, Space.World);
        }
    }
}