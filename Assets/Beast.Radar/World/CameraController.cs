using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Mathematics;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.World
{
    public class CameraController : Initializable, IFrameBeginUpdate, ICameraController
    {
        public CameraControllerState State { get; private set; }
        
        private readonly WorldCameraConfig _config;

        public CameraController(WorldCameraConfig config)
        {
            _config = config;
        }
        
        public UniTask<InitializationResult> Initialize()
        {
            State = new CameraControllerState
            {
                position = float2.zero,
                zoom01 = 1
            };
            
            return InitializationResult.SuccessNotAsync();
        }
        
        public void OnFrameBegin()
        {
            CameraControllerState cameraState = State;
            
            float2 motion = float2.zero;
            motion.y = Input.GetAxis("Vertical");
            motion.x = Input.GetAxis("Horizontal");
            motion = BMath.NormalizeSafe(motion);

            float zoomDistance = BMath.Lerp(_config.zoomRange, cameraState.zoom01);
            cameraState.position += motion * (zoomDistance * Time.deltaTime * _config.motionSpeed);

            float zoomDelta = _config.zoomSpeed * Time.deltaTime;
            zoomDelta *= _config.zoomCurve.Evaluate(cameraState.zoom01);
            zoomDelta = BMath.Max(0.001f, zoomDelta);
            zoomDelta *= Input.GetAxis("Mouse ScrollWheel");
            
            cameraState.zoom01 = BMath.Clamp01(cameraState.zoom01 - zoomDelta);

            if (Input.GetMouseButtonDown(0))
            {
                float2 mousePosition = new (Input.mousePosition.x, Input.mousePosition.y);
                float3 worldPoint = ScreenPointToWorld(mousePosition);
                
                BeastLog.Error(this, "Clicked at " + worldPoint);
            }

            State = cameraState;
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

            return new float3(screen.x, 0, screen.y);
        }
    }
}