using System;
using Beast.Claw.Common;
using Beast.Claw.Debugging;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraController : ICameraController, IFrameBeginUpdate, IFrameEndUpdate,IGizmos
    {
        public CameraControllerState State { get; private set; }

        private readonly CameraConfig _config;
        private CameraInput _input;

        private float _startInertiaTime;
        private Vector3 _startInertia;
        private Vector3 _inertia;
        
        public CameraController(CameraConfig config)
        {
            _config = config;
        }

        public UniTask<InitializationResult> Initialize()
        {
            CameraView view = BeastScene.NewInstance
            (
                _config.viewPrefab,
                _config.startPosition,
                Quaternion.identity
            );

            _input = new CameraInput();
            _input.Initialize();

            return InitializationResult.SuccessNotAsync();
        }

        public void OnFrameBegin()
        {
            _input.EarlyInput();
            ProcessCamera(_input.CurrentScreenPoint, _input.ZoomDelta);
        }

        public void OnFrameEnd()
        {
            _input.LateInput();
        }
        
        private void ProcessCamera(Vector3 currentScreenPoint, float zoomDelta)
        {
            if (_input.LastScreenPoint.magnitude == 0)
            {
                return;
            }

            Move(_input.ScreenPointDelta);
            Zoom(zoomDelta, currentScreenPoint);
            Inertia();
        }

        private void Move(Vector3 screenPointDelta)
        {
            if (!_input.IsPressMove || _input.LastTouchCount != _input.CurrentTouchCount)
            {
                return;
            }

            if (BorderCheck(State.position - screenPointDelta.AsFloat2() * _config.motionSpeed))
            {
                RefreshStatePosition(State.position - screenPointDelta.AsFloat2() * _config.motionSpeed);
                _startInertia = screenPointDelta;
                _startInertiaTime = -1;
            }
        }
        
        private void Zoom(float inputZoom, Vector3 inputPoint)
        {
            if (inputZoom == 0)
            {
                return;
            }
            
            Vector3 worldPointBeforeZoom = ScreenPointToWorld(inputPoint.AsFloat2());

            float newZoom = State.zoom01 - inputZoom * _config.zoomSpeed;
            
            float clampZoom = Mathf.Clamp(
                newZoom,
                _config.zoomRange.min,
                _config.zoomRange.max
            );

            RefreshStateZoom(clampZoom);

            Debug.Log($"Zoom Value: {inputZoom}, New Zoom: {clampZoom}");

            Vector3 worldPointAfterZoom = ScreenPointToWorld(inputPoint.AsFloat2());

            Vector3 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;

            float2 newPosition = State.position + pointDelta.AsFloat2();
          
            Debug.Log($"Point Delta: {pointDelta}");
            if (BorderCheck(newPosition))
            {
                RefreshStatePosition(newPosition);
                Debug.Log($"New Position Updated: {newPosition}");
            }
            else
            {
                Debug.Log("Position not updated due to border check.");
            }
        }

        private void Inertia()
        {
            if (_input.IsPressMove)
            {
                return;
            }

            if (Math.Abs(_startInertiaTime + 1f) < 0.0001f)
            {
                _startInertiaTime = Time.time;
            }

            float2 position = State.position - _inertia.AsFloat2();
            if (BorderCheck(position))
            {
                RefreshStatePosition(position);
            }

            _inertia = _config.inertiaDeceleration.Evaluate(Time.time - _startInertiaTime) * _startInertia;
        }

        private bool BorderCheck(float2 position)
        {
            var x = position.x;
            var y = position.y;

            var a = _config.ellipseSize.x;
            var b = _config.ellipseSize.y;

            var result = x * x / (a * a) + y * y / (b * b);

            return result <= 1.0f;
        }

        private float3 ScreenPointToWorld(float2 screenPoint)
        {
            float2 viewport = new ( screenPoint.x / Screen.width, screenPoint.y / Screen.height);

            return ViewportToWorld(viewport);
        }

        private float3 ViewportToWorld(float2 viewport)
        {
            float viewDistance = BMath.Lerp(_config.zoomRange, State.zoom01);
            float screenAspect = Screen.width / (float)Screen.height;
            float2 bottomLeft = State.position - new float2(screenAspect, 1) * viewDistance * 0.5f;
            float2 screen = bottomLeft + viewport * new float2(screenAspect, 1) * viewDistance;

            return new float3(screen.x, screen.y, 0);
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
                var angle = i * angleIncrement * Mathf.Deg2Rad;
                var x = _config.ellipseSize.x * Mathf.Cos(angle);
                var y = _config.ellipseSize.y * Mathf.Sin(angle);
                var point = new Vector3(x, y, 0);

                if (previousPoint != Vector3.zero)
                {
                    Gizmos.DrawLine(previousPoint, point);
                }

                previousPoint = point;
            }
        }
    }
}