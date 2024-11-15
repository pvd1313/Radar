using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Input;
using Beast.Claw.Math;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraTransformController : Controller, IGameCycleListener, IInputConsumer
    {
        public EInputAxis SupportedAxis => EInputAxis.Drag | EInputAxis.Zoom;
        
        private readonly ICameraTransform _transform;
        private readonly CameraConfig _cameraConfig;
        private readonly IInputManager _inputManager;

        private float2 _dragCameraPosition;

        public CameraTransformController(
            ICameraTransform transform, 
            CameraConfig cameraConfig, 
            IInputManager inputManager)
        {
            _transform = transform;
            _cameraConfig = cameraConfig;
            _inputManager = inputManager;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
        }
        
        public bool TryCaptureAxis(PackedAxis axis)
        {
            return _transform.IsReady;
        }

        public void UpdateAxis(PackedAxis axis)
        {
            if (axis.GetAxisType() == EInputAxis.Drag)
            {
                axis.Unpack(out DragAxis drag);

                if (drag.status == EAxisStatus.Starting)
                {
                    _dragCameraPosition = _transform.Position;
                }

                if (drag.status != EAxisStatus.Disabled)
                {
                    float2 delta = _transform.ViewportToWorld(drag.startPosition) - 
                                   _transform.ViewportToWorld(drag.position);

                    _transform.SetPosition(_dragCameraPosition + delta);
                }
            }

            if (axis.GetAxisType() == EInputAxis.Zoom)
            {
                axis.Unpack(out ZoomAxis zoom);
                
                float2 beforeZoom = _transform.ViewportToWorld(zoom.position);
            
                float factor = BMath.Lerp(1e-4f, 15f, _transform.Zoom01);
                float zoomValue = zoom.delta * factor;

                float smoothValue = BMath.Lerp(
                    _transform.Zoom01, 
                    _transform.Zoom01 + zoomValue, 
                    _cameraConfig.zoomSmoothSpeed * (1f / 60f)) - _transform.Zoom01;
            
                _transform.AddZoom(smoothValue);
            
                float2 afterZoom = _transform.ViewportToWorld(zoom.position);
                _transform.AddPosition(beforeZoom - afterZoom, Space.World);
            }
        }

        private UniTask _warmup(ConcurrentContext context)
        {
            _inputManager.RegisterConsumer(this, InputConsumerPriority.WORLD_MAP);
            
            return UniTask.CompletedTask;
        }
    }
}