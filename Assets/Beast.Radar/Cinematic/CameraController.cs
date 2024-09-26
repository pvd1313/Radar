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
        private float2 _startInertia;
        private float2 _inertia;
        
        public CameraController(CameraConfig config)
        {
            _config = config;
        }

        public UniTask<InitializationResult> Initialize()
        {
            RefreshStatePosition(float2.zero);
            RefreshStateZoom(_config.zoomRange.min);
            
            BeastScene.NewInstance
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
            ProcessCamera();
        }

        public void OnFrameEnd()
        {
            _input.LateInput();
        }
        
        private void ProcessCamera()
        {
            if (_input.LastScreenPoint.magnitude == 0)
            {
                return;
            }

            Move();
            Zoom();
            Inertia();
        }

        private void Move()
        {
            if (!_input.IsPressMove || _input.LastTouchCount != _input.CurrentTouchCount)
            {
                return;
            }

            float2 newPosition = State.position - _input.ScreenPointDelta.AsFloat2() * _config.motionSpeed; 
            if (BorderCheck(newPosition))
            {
                RefreshStatePosition(newPosition);
                _startInertia = _input.ScreenPointDelta.AsFloat2();
                _startInertiaTime = -1;
            }
        }
        
        private void Zoom()
        {
            if (_input.Zoom == 0)
            {
                return;
            }

            float2 screenPoint = _input.ScreenPoint.AsFloat2();
            float2 worldPointBeforeZoom = ScreenPointToWorld(screenPoint);
            
            float newZoom = State.zoom01 - _input.Zoom * _config.zoomSpeed;
            float clampZoom = Mathf.Clamp(
                newZoom,
                _config.zoomRange.min,
                _config.zoomRange.max
            );
            RefreshStateZoom(clampZoom);
            //Debug.Log($"Zoom Value: {inputZoom}, New Zoom: {clampZoom}");

            float2 worldPointAfterZoom = ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom; 
            float2 newPosition = State.position + pointDelta;
            //Debug.Log($"Point Delta: {pointDelta}");
            if (BorderCheck(newPosition))
            {
                RefreshStatePosition(newPosition);
               // Debug.Log($"New Position Updated: {newPosition}");
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

            float2 position = State.position - _inertia;
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

        private float2 ScreenPointToWorld(float2 screenPoint)
        {
            float2 viewport = new ( screenPoint.x / Screen.width, screenPoint.y / Screen.height);

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