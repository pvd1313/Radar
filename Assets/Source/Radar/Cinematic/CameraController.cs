using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : ICameraController
    {
        public CameraControllerState State { get; private set; }

        private readonly CameraConfig _config;
        private readonly IInputController _input;
        private readonly IPlayerInputProvider _inputProvider;
        
        private CameraParams _currentParams;

        public CameraController(CameraConfig config, 
            IInputController inputController,
            IPlayerInputProvider inputProvider)
        {
            _config = config;
            _input = inputController;
            _inputProvider = inputProvider;
        }

        public UniTask<InitializationResult> Initialize()
        {
            RefreshStatePosition(_config.startPosition);
            RefreshStateZoom(_config.zoomRange.min);

            BeastScene.Instantiate
            (
                _config.viewPrefab,
                float3.zero,
                quaternion.identity
            );

            _currentParams = _config.GetParams(_inputProvider.Scheme);
            SubscribeToEvents(true);

            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            SubscribeToEvents(false);
            return DispositionResult.SuccessNotAsync();
        }

        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _input.ChangedEarlyValues += ProcessCamera;
                _inputProvider.ChangedScheme += OnChangedInputScheme;
            }
            else
            {
                _input.ChangedEarlyValues -= ProcessCamera;
                _inputProvider.ChangedScheme -= OnChangedInputScheme;
            }
        }

        private void OnChangedInputScheme(EInputScheme scheme)
        {
            _currentParams = _config.GetParams(scheme);
          //  Debug.Log($"Change param {_currentParams.zoomSpeed}");
        }
        
        private void ProcessCamera()
        {
            Move();
            Zoom();
        }

        private void Move()
        {
            if (!_input.IsPressMove || _input.LastTouchCount != _input.CurrentTouchCount)
            {
                return;
            }

            float zoomDistance = BMath.Lerp(_config.zoomRange, State.zoom01) * 0.5f;
            float2 direction = _input.MoveDelta * _currentParams.motionSpeed;
            float2 newPosition = State.position - direction * zoomDistance * Time.deltaTime;

            RefreshStatePosition(newPosition);
        }

        private void Zoom()
        {
            if (_input.Zoom == 0)
            {
                return;
            }

            float2 screenPoint = _input.ScreenPoint;
            float2 worldPointBeforeZoom = ScreenPointToWorld(screenPoint);

            float zoomDelta = _currentParams.zoomSpeed * Time.deltaTime;
            zoomDelta *= _config.zoomCurve.Evaluate(State.zoom01);
            zoomDelta = BMath.Max(0.000f, zoomDelta);
            zoomDelta *= _input.Zoom;

            RefreshStateZoom(BMath.Clamp01(State.zoom01 - zoomDelta));

            if (_inputProvider.Scheme is EInputScheme.Gamepad)
            {
                return;
            }
            
            float2 worldPointAfterZoom = ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;
            float2 newPosition = State.position + pointDelta;

            RefreshStatePosition(newPosition);
        }

        private float2 ScreenPointToWorld(float2 screenPoint)
        {
            float2 viewport = new(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
            return ViewportToWorld(viewport);
        }

        private float2 ViewportToWorld(float2 viewport)
        {
            float viewDistance = BMath.Lerp(_config.zoomRange, State.zoom01);
            float screenAspect = Screen.width / (float)Screen.height;
            float2 bottomLeft = State.position - new float2(screenAspect, 1) * viewDistance * 0.5f;
            float2 screen = bottomLeft + viewport * new float2(screenAspect, 1) * viewDistance;

            return new float2(screen.x, screen.y);
        }

        private void RefreshStatePosition(float2 pos)
        {
            CameraControllerState state = State;
            state.position = pos;
            State = state;
        }

        private void RefreshStateZoom(float zoom)
        {
            CameraControllerState state = State;

            state.zoom01 = zoom;
            State = state;
        }
    }
}