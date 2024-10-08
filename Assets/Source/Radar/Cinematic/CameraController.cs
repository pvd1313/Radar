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
    public class CameraController : IGameCycleListener
    {
        private readonly CameraConfig _config;
        private readonly ICameraInputController _input;
        private readonly IPlayerInputProvider _inputProvider;
        private readonly ICameraTransform _transform;

        private CameraMotionParameters _motion;
        private float _maxViewDistance;

        public CameraController(
            CameraConfig config,
            ICameraInputController inputController,
            IPlayerInputProvider inputProvider,
            ICameraTransform transform)
        {
            _config = config;
            _input = inputController;
            _inputProvider = inputProvider;
            _transform = transform;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
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

            if (!_transform.IsReady)
            {
                return;
            }

            Move();
            Zoom();
        }

        private void Move()
        {
            if (!_input.IsPressMove)
            {
                return;
            }

            // TODO : What was the magic 0.2 number?
            // https://risenbeast.youtrack.cloud/issue/R-36/Floating-Zoom-Distance
            // float zoomDistance = BMath.Lerp(_config.minViewDistance, _maxViewDistance, state.zoom01 * 0.2f);
            float zoomDistance = _transform.TargetViewDistance * BMath.Cos(_transform.CurrentRotationRad.roll);
            float2 moveDelta = float2.zero;
            
            if (_input.MoveDelta.Equals(float2.zero))
            {
                // TODO : This is not correct. Zoom distance will float, we need to remember positions instead. 
                //      : https://risenbeast.youtrack.cloud/issue/R-36/Floating-Zoom-Distance
                // float2 a = _transform.ScreenPointToWorld(_input.ScreenPoint);
                // float2 b = _transform.ScreenPointToWorld(_input.LastScreenPoint);
                // moveDelta = a - b;
            }
            else
            {
                moveDelta = _input.MoveDelta * _motion.motionSpeed * zoomDistance * -1;
            }
            
            BDebug.TrackFloat2(this, "moveDelta", moveDelta);
            
            _transform.AddLocalPosition(moveDelta);
        }

        private void Zoom()
        {
            if (_input.ScrollDelta == 0)
            {
                return;
            }

            float2 screenPoint = _input.ScreenPoint;
            float2 worldPointBeforeZoom = _transform.ScreenPointToWorld(screenPoint);

            float zoomDelta = _motion.zoomSpeed * Time.deltaTime;
            zoomDelta *= _config.zoomCurve.Evaluate(_transform.Zoom01);
            zoomDelta = BMath.Max(0, zoomDelta);
            zoomDelta *= _input.ScrollDelta;
            _transform.AddZoom(zoomDelta * -1);

            if (_inputProvider.Scheme is EInputScheme.Gamepad)
            {
                return;
            }

            float2 worldPointAfterZoom = _transform.ScreenPointToWorld(screenPoint);
            float2 pointDelta = worldPointBeforeZoom - worldPointAfterZoom;
            _transform.AddLocalPosition(pointDelta);
        }
    }
}