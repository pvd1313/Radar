using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : ICameraController, IGameCycleListener
    {
        public float2 Position => _game.camera.position;

        public float ViewDistance => BMath.Lerp(_config.minViewDistance, _maxViewDistance, _game.camera.zoom01);

        public IsometricCameraRotation RotationRad => _config.rotationDeg * BMath.Deg2Rad;

        private readonly GameModel _game;
        private readonly IScreen _screen;
        private readonly CameraConfig _config;
        private readonly ICameraInputController _input;
        private readonly IPlayerInputProvider _inputProvider;

        private CameraMotionParameters _motion;
        private float _maxViewDistance;

        public CameraController(
            GameModel game,
            IScreen screen,
            CameraConfig config,
            ICameraInputController inputController,
            IPlayerInputProvider inputProvider)
        {
            _game = game;
            _screen = screen;
            _config = config;
            _input = inputController;
            _inputProvider = inputProvider;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.GameLoading, GameLoading);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }

        private UniTask<OperationResult> Warmup()
        {
            BeastScene.InstantiateMono
            (
                this,
                _config.viewPrefab,
                float3.zero,
                quaternion.identity
            );

            _motion = _config.GetMotionParameters(_inputProvider.Scheme);
            
            SubscribeInput();
            
            return OperationResult.SuccessSync();
        }
        
        private UniTask<OperationResult> GameLoading()
        {
            _maxViewDistance = BMath.Max(_game.terrain.size.x, _game.terrain.size.y) * _config.maxViewDistance01;

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeInput();

            return OperationResult.SuccessSync();
        }

        private void SubscribeInput()
        {
            _input.ChangedEarlyValues += ProcessCamera;
            _inputProvider.ChangedScheme += OnChangedInputScheme;
        }

        private void UnsubscribeInput()
        {
            _input.ChangedEarlyValues -= ProcessCamera;
            _inputProvider.ChangedScheme -= OnChangedInputScheme;
        }

        private void OnChangedInputScheme(EInputScheme scheme)
        {
            _motion = _config.GetMotionParameters(scheme);
        }

        private void ProcessCamera()
        {
            if (_input.LastTouchCount != _input.CurrentTouchCount)
            {
                return;
            }

            Move(ref _game.camera);
            Zoom(ref _game.camera);
        }

        private void Move(ref CameraState state)
        {
            if (!_input.IsPressMove)
            {
                return;
            }

            float zoomDistance = BMath.Lerp(_config.minViewDistance, _maxViewDistance, state.zoom01 * 0.2f);
            float2 moveDelta;
            
            if (_input.MoveDelta.Equals(float2.zero))
            {
                moveDelta = ScreenPointToWorld(_input.ScreenPoint) - ScreenPointToWorld(_input.LastScreenPoint);
            }
            else
            {
                moveDelta = _input.MoveDelta * _motion.motionSpeed * zoomDistance;
            }
            
            state.position -= moveDelta;
        }

        private void Zoom(ref CameraState state)
        {
            if (_input.ScrollDelta == 0)
            {
                return;
            }

            float2 screenPoint = _input.ScreenPoint;
            float2 worldPointBeforeZoom = ScreenPointToWorld(screenPoint);

            float zoomDelta = _motion.zoomSpeed * Time.deltaTime;
            zoomDelta *= _config.zoomCurve.Evaluate(state.zoom01);
            zoomDelta = BMath.Max(0, zoomDelta);
            zoomDelta *= _input.ScrollDelta;
            state.zoom01 = BMath.Clamp01(state.zoom01 - zoomDelta);

            if (_inputProvider.Scheme is EInputScheme.Gamepad)
            {
                return;
            }

            float2 worldPointAfterZoom = ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;
            state.position += pointDelta;
        }

        private float2 ScreenPointToWorld(float2 screenPoint)
        {
            float2 viewport = new(screenPoint.x / _screen.Width, screenPoint.y / _screen.Height);
            
            return ViewportToWorld(viewport);
        }

        private float2 ViewportToWorld(float2 viewport)
        {
            CameraState state = _game.camera;
            
            float viewDistance = BMath.Lerp(_config.minViewDistance, _maxViewDistance, state.zoom01);
            float2 bottomLeft = state.position - new float2(_screen.Aspect, 1) * viewDistance * 0.5f;
            float2 screen = bottomLeft + viewport * new float2(_screen.Aspect, 1) * viewDistance;

            return new float2(screen.x, screen.y);
        }
    }
}