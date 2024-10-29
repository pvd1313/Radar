using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : IGameCycleListener
    {
        private readonly CameraConfig _cameraConfig;
        private readonly IInputSystem _inputSystem;
        private readonly ICameraTransform _transform;
        private readonly IScreen _screen;

        private CameraMotionParameters _motion;
        private float _maxViewDistance;

        public CameraController(
            CameraConfig cameraConfig,
            IInputSystem inputSystem,
            ICameraTransform transform,
            IScreen screen)
        {
            _cameraConfig = cameraConfig;
            _inputSystem = inputSystem;

            _transform = transform;
            _screen = screen;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }

        private UniTask<ActionResult> _warmup(ConcurrentContext context)
        {
            _motion = _cameraConfig.GetMotionParameters(_inputSystem.Scheme.Current);

            _inputSystem.GameInput.ChangedEarlyValues += _processCamera;
            _inputSystem.Scheme.Changed += _onChangedInputActionsScheme;

            return ActionResult.SuccessSyncronous();
        }

        private void _onChangedInputActionsScheme(EInputScheme scheme)
        {
            _motion = _cameraConfig.GetMotionParameters(scheme);
        }

        private void _processCamera()
        {
            if (_inputSystem.GameInput.LastTouchCount.Value != _inputSystem.GameInput.CurrentTouchCount.Value)
            {
                return;
            }

            if (!_transform.IsReady)
            {
                return;
            }

            _move();
            _zoom();
        }

        private void _move()
        {
            if (!_inputSystem.GameInput.IsPressMove)
            {
                return;
            }
            
            float zoomDistance = _transform.ViewDistance * BMath.Cos(_transform.RotationRad.roll);
            
            if (_inputSystem.GameInput.MoveDelta.State == EButtonState.Default)
            {
                float2 a = _transform.ScreenToWorld(_inputSystem.GameInput.ScreenPoint.Value);
                float2 b = _transform.ScreenToWorld(_inputSystem.GameInput.LastScreenPoint.Value);
                float2 moveDelta = b - a;
                _transform.AddPosition(moveDelta, Space.World);

                float2 viewport = _inputSystem.GameInput.ScreenPoint.Value / (float2)_screen.Size;

                BDebug.Track(this, "viewportPoint", viewport);
                BDebug.Track(this, "screenPoint", _inputSystem.GameInput.ScreenPoint.Value);
                BDebug.Track(this, "worldPoint", a);
            }
            else
            {
                float2 moveDelta = _inputSystem.GameInput.MoveDelta.Value * _motion.motionSpeed * zoomDistance * -1;
                _transform.AddPosition(moveDelta, Space.Self);
            }
        }

        private void _zoom()
        {
            if (_inputSystem.GameInput.Scroll.State == EButtonState.Default)
            {
                return;
            }

            float2 screenPoint = _inputSystem.GameInput.ScreenPoint.Value;
            float2 worldPointBeforeZoom = _transform.ScreenToWorld(screenPoint);
            
            float distanceFactor = BMath.Lerp(0.0001f, 15f, _transform.Zoom01);
            float zoomValue = _inputSystem.GameInput.Scroll.Value * distanceFactor * _motion.zoomSpeed * Time.deltaTime;
            _transform.AddZoom(-zoomValue);
            
            float2 worldPointAfterZoom = _transform.ScreenToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;

            _transform.AddPosition(pointDelta, Space.World);
        }
    }
}