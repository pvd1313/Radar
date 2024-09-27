using Beast.Claw.Common;
using Beast.Claw.Debugging;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : ICameraController, IGizmos
    {
        public CameraControllerState State { get; private set; }

        private readonly CameraConfig _config;
        private readonly IInputController _input;

        public CameraController(CameraConfig config, IInputController inputController)
        {
            _config = config;
            _input = inputController;
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

            _input.ChangedEarlyValues += OnChangedEarlyInputValues;

            return InitializationResult.SuccessNotAsync();
        }

        public UniTask<DispositionResult> Dispose()
        {
            _input.Dispose();
            return DispositionResult.SuccessNotAsync();
        }

        private void OnChangedEarlyInputValues()
        {
            ProcessCamera();
        }

        private void ProcessCamera()
        {
            if (_input.LastScreenPoint.Magnitude() == 0)
            {
                return;
            }

            Move();
            Zoom();
        }

        private void Move()
        {
            if (!_input.IsPressMove || _input.LastTouchCount != _input.CurrentTouchCount)
            {
                return;
            }

            float zoomDistance = BMath.Lerp(_config.zoomRange, State.zoom01);
            float2 direction = _input.MoveDelta.Equals(float2.zero) 
                ?  ScreenPointToWorld(_input.ScreenPoint) - ScreenPointToWorld(_input.LastScreenPoint) 
                : _input.MoveDelta;
            float2 newPosition = State.position - direction * zoomDistance * Time.deltaTime * _config.motionSpeed;

            if (BorderCheck(newPosition))
            {
                RefreshStatePosition(newPosition);
            }
        }

        private void Zoom()
        {
            if (_input.Zoom == 0)
            {
                return;
            }

            float2 screenPoint = _input.ScreenPoint;
            float2 worldPointBeforeZoom = ScreenPointToWorld(screenPoint);

            float zoomDelta = _config.zoomSpeed * Time.deltaTime;
            zoomDelta *= _config.zoomCurve.Evaluate(State.zoom01);
            zoomDelta = BMath.Max(0.001f, zoomDelta);
            zoomDelta *= _input.Zoom;

            RefreshStateZoom(BMath.Clamp01(State.zoom01 - zoomDelta));

            float2 worldPointAfterZoom = ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;
            float2 newPosition = State.position + pointDelta;

            if (BorderCheck(newPosition))
            {
                RefreshStatePosition(newPosition);
            }
        }

        private bool BorderCheck(float2 position)
        {
            float x = position.x;
            float y = position.y;

            float a = _config.ellipseSize.x;
            float b = _config.ellipseSize.y;

            float result = x * x / (a * a) + y * y / (b * b);

            return result <= 1.0f;
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

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            const int segments = 100;
            const float angleIncrement = 360.0f / segments;

            var previousPoint = Vector3.zero;

            for (var i = 0; i < segments; i++)
            {
                float angle = i * angleIncrement * Mathf.Deg2Rad;
                float x = _config.ellipseSize.x * Mathf.Cos(angle);
                float y = _config.ellipseSize.y * Mathf.Sin(angle);
                float3 point = new float3(x, y, 0);

                if (!previousPoint.Equals(Vector3.zero))
                {
                    Gizmos.DrawLine(previousPoint, point);
                }

                previousPoint = point;
            }
        }
    }
}