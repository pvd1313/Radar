using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : IGameCycleListener
    {
        private readonly CameraConfig _cameraConfig;
        private readonly ICameraInputController _input;
        private readonly IPlayerInputProvider _inputProvider;
        private readonly ICameraTransform _transform;
        private readonly PrefabLibrary _prefabs;
        private readonly IScreen _screen;

        private CameraMotionParameters _motion;
        private float _maxViewDistance;

        public CameraController(
            CameraConfig cameraConfig,
            ICameraInputController inputController,
            IPlayerInputProvider inputProvider,
            ICameraTransform transform,
            PrefabLibrary prefabs,
            IScreen screen)
        {
            _cameraConfig = cameraConfig;
            _input = inputController;
            _inputProvider = inputProvider;
            _transform = transform;
            _prefabs = prefabs;
            _screen = screen;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        private UniTask<OperationResult> Warmup()
        {
            BScene.InstantiateMono
            (
                this,
                _prefabs.cameraPrefab,
                new float3(0, 0, -1),
                quaternion.identity
            );

            _motion = _cameraConfig.GetMotionParameters(_inputProvider.Scheme);
            
            _subscribeInput();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            _unsubscribeInput();

            return OperationResult.SuccessSync();
        }

        private void _subscribeInput()
        {
            _input.ChangedEarlyValues += _processCamera;
            _inputProvider.ChangedScheme += _onChangedInputScheme;
        }

        private void _unsubscribeInput()
        {
            _input.ChangedEarlyValues -= _processCamera;
            _inputProvider.ChangedScheme -= _onChangedInputScheme;
        }

        private void _onChangedInputScheme(EInputScheme scheme)
        {
            _motion = _cameraConfig.GetMotionParameters(scheme);
        }

        private void _processCamera()
        {
            if (_input.LastTouchCount != _input.CurrentTouchCount)
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
            if (!_input.IsPressMove)
            {
                return;
            }
            
            float zoomDistance = _transform.ViewDistance * BMath.Cos(_transform.RotationRad.roll);
            
            if (_input.MoveDelta.Equals(float2.zero))
            {
                float2 a = _transform.ScreenPointToWorld(_input.ScreenPoint);
                float2 b = _transform.ScreenPointToWorld(_input.LastScreenPoint);
                float2 moveDelta = b - a;
                _transform.AddPosition(moveDelta, Space.World);

                float2 viewport = _input.ScreenPoint / (float2)_screen.Size;

                BDebug.Track(this, "viewportPoint", viewport);
                BDebug.Track(this, "screenPoint", _input.ScreenPoint);
                BDebug.Track(this, "worldPoint", a);
            }
            else
            {
                float2 moveDelta = _input.MoveDelta * _motion.motionSpeed * zoomDistance * -1;
                _transform.AddPosition(moveDelta, Space.Self);
            }
        }

        private void _zoom()
        {
            if (_input.ScrollDelta == 0)
            {
                return;
            }

            float2 screenPoint = _input.ScreenPoint;
            float2 worldPointBeforeZoom = _transform.ScreenPointToWorld(screenPoint);
            
            float distanceFactor = BMath.Lerp(0.0001f, 15f, _transform.Zoom01);
            float zoomValue = _input.ScrollDelta * distanceFactor * _motion.zoomSpeed * Time.deltaTime;
            _transform.AddZoom(-zoomValue);
            
            float2 worldPointAfterZoom = _transform.ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;

            _transform.AddPosition(pointDelta, Space.World);
        }
    }
}