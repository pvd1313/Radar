using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraTransform : IGameCycleListener, ICameraTransform
    {
        public bool IsReady { get; private set; }
        
        public IsometricCameraRotation MaxRotationRad => _config.isometricRotationDeg * BMath.Deg2Rad;
        public float2 Position => _position;
        public IsometricCameraRotation CurrentRotationRad => _rotationRad;
        public float Zoom01 => _zoom01;
        public float TargetViewDistance => _viewDistance;

        private readonly IScreen _screen;
        private readonly CameraTransformConfig _config;
        private readonly GameModel _game;

        private IsometricCameraRotation _rotationRad;
        private float _viewDistance;
        private float _maxViewDistance;
        private float _zoom01;
        private float _zoomMax;
        private float2 _position;
        private float _pitchMax;
        private float _rollMax;
        
        public CameraTransform(IScreen screen, CameraTransformConfig config, GameModel game)
        {
            _screen = screen;
            _config = config;
            _game = game;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.GameLoading, GameLoading);
        }
        
        public float2 ScreenPointToWorld(float2 screenPoint)
        {
            return ViewportToWorld(screenPoint / _screen.Size);
        }

        public void AddZoom(float zoomDelta)
        {
            InternalSetZoom(_zoom01 + zoomDelta);
        }

        public void AddLocalPosition(float2 delta)
        {
            _position += TransformVector(delta);
        }

        public float2 ViewportToWorld(float2 viewport)
        {
            float2 bottomLeft = _position - new float2(_screen.Aspect, 1) * _viewDistance * 0.5f;
            float2 screen = bottomLeft + viewport * new float2(_screen.Aspect, 1) * _viewDistance;

            return new float2(screen.x, screen.y);
        }

        private float2 TransformVector(float2 vector)
        {
            double2x2 rotation = float2x2.Rotate(_rotationRad.roll);
            double2x2 scale = float2x2.Scale(1, BMath.Cos(_rotationRad.pitch));

            return (float2) scale.Inverse().Multiply(rotation.Multiply(vector));
        }

        private UniTask<OperationResult> GameLoading()
        {
            _position = _game.camera.position;

            _zoomMax = _config.enablePlanetMode ? 0.25f : _config.plainModeViewDistance01;
            _pitchMax = _config.isometricRotationDeg.pitch * BMath.Deg2Rad;
            _rollMax = _config.isometricRotationDeg.roll * BMath.Deg2Rad;
            _maxViewDistance = BMath.Max(_game.terrain.size.x, _game.terrain.size.y) * _zoomMax;

            InternalSetZoom(_game.camera.zoom01);
            
            IsReady = true;
            
            return OperationResult.SuccessSync();
        }

        private void InternalSetZoom(float zoom)
        {
            _zoom01 = BMath.Clamp01(zoom);
            _viewDistance = BMath.Lerp(_config.minViewDistance, _maxViewDistance, _zoom01);
            
            // Convert to full isometric on half max zoom
            float isometric01 = BMath.Clamp01((1 - _zoom01 / (_zoomMax * 0.5f)));

            _rotationRad = MaxRotationRad;

            _rotationRad = new IsometricCameraRotation
            {
                pitch = BMath.Lerp(0, _pitchMax, isometric01),
                roll = BMath.Lerp(0, _rollMax, isometric01)
            };
        }
    }
}