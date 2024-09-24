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
                position = WorldMapRender.WorldSize / 2,
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

            State = cameraState;
        }
    }
}