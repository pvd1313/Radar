using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class BeastCameraController : IBeastCameraController, IBeastGameCycleListener
    {
        public float2 Position => _game.camera.position;

        public float ViewDistance => BMath.Lerp(_config.minViewDistance, _maxViewDistance, _game.camera.zoom01);
        
        private readonly BeastGameModel _game;
        private readonly BeastCameraConfig _config;
        private readonly IBeastCameraInputController _input;
        private readonly IBeastPlayerInputProvider _inputProvider;

        private BeastCameraMotionParameters _motion;
        private float _maxViewDistance;

        public BeastCameraController(
            BeastGameModel game,
            BeastCameraConfig config,
            IBeastCameraInputController inputController,
            IBeastPlayerInputProvider inputProvider)
        {
            _game = game;
            _config = config;
            _input = inputController;
            _inputProvider = inputProvider;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.GameLoading, GameLoading);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
        }

        private UniTask<BeastInitializationResult> Warmup()
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
            
            return BeastInitializationResult.SuccessSync();
        }
        
        private UniTask<BeastInitializationResult> GameLoading()
        {
            _maxViewDistance = BMath.Max(_game.terrain.size.x, _game.terrain.size.y) * 0.5f;

            return BeastInitializationResult.SuccessSync();
        }

        private UniTask<BeastInitializationResult> Dispose()
        {
            UnsubscribeInput();

            return BeastInitializationResult.SuccessSync();
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

        private void OnChangedInputScheme(EBeastInputScheme scheme)
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

        private void Move(ref BeastCameraState state)
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

        private void Zoom(ref BeastCameraState state)
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

            if (_inputProvider.Scheme is EBeastInputScheme.Gamepad)
            {
                return;
            }

            float2 worldPointAfterZoom = ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;
            state.position += pointDelta;
        }

        private float2 ScreenPointToWorld(float2 screenPoint)
        {
            float2 viewport = new(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
            
            return ViewportToWorld(viewport);
        }

        private float2 ViewportToWorld(float2 viewport)
        {
            BeastCameraState state = _game.camera;
            
            float viewDistance = BMath.Lerp(_config.minViewDistance, _maxViewDistance, state.zoom01);
            float screenAspect = Screen.width / (float)Screen.height;
            float2 bottomLeft = state.position - new float2(screenAspect, 1) * viewDistance * 0.5f;
            float2 screen = bottomLeft + viewport * new float2(screenAspect, 1) * viewDistance;

            return new float2(screen.x, screen.y);
        }
    }
}