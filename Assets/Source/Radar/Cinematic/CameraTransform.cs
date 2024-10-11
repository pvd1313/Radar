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
        
        public float2 Position => _position;
        public IsometricCameraRotation RotationRad => _rotationRad;
        public float Zoom01 => _zoom01;
        public float ViewDistance => _viewDistance;

        private readonly IScreen _screen;
        private readonly CameraTransformConfig _config;
        private readonly GameModel _game;

        private IsometricCameraRotation _rotationRad;
        private float _viewDistance;
        private float _maxViewDistance;
        private float _zoom01;
        private float _zoomMax;
        private float2 _position;
        
        public CameraTransform(IScreen screen, CameraConfig config, GameModel game)
        {
            _screen = screen;
            _config = config.transform;
            _game = game;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.GameLoading, _gameLoading);
        }

        private UniTask<OperationResult> _warmup()
        {
            _rotationRad = _config.isometricRotationDeg * BMath.Deg2Rad;

            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _gameLoading()
        {
            _position = _game.camera.position;

            _zoomMax = _config.enablePlanetMode ? 0.25f : _config.plainModeViewDistance01;
            _maxViewDistance = BMath.Max(_game.terrain.size.x, _game.terrain.size.y) * _zoomMax;

            _setZoom(_game.camera.zoom01);
            
            IsReady = true;
            
            return OperationResult.SuccessSync();
        }

        public void AddPosition(float2 delta, Space space)
        {
            if (space == Space.Self)
            {
                _position += _localToWorld(delta);
            }
            else
            {
                _position += delta;
            }
            
            _debugTrack();
        }

        public void AddZoom(float zoomDelta)
        {
            _setZoom(_zoom01 + zoomDelta);
        }
        
        public float2 ScreenPointToWorld(float2 screenPoint)
        {
            return ViewportToWorld(screenPoint / _screen.Size);
        }

        public float2 ViewportToWorld(float2 viewport)
        {
            float viewDistance = _viewDistance * BMath.Cos(_rotationRad.roll);
            
            float2 bottomLeft = _position - _localToWorld(new float2(_screen.Aspect, 1) * viewDistance * 0.5f);
            float2 screen = bottomLeft + _localToWorld(viewport * new float2(_screen.Aspect, 1) * viewDistance);

            return new float2(screen.x, screen.y);
        }

        private float2 _localToWorld(float2 vector)
        {
            float2x2 rotation = float2x2.Rotate(_rotationRad.roll * -1);
            float2x2 scale = float2x2.Scale(1, BMath.Cos(_rotationRad.pitch));

            return rotation.Multiply((scale.Inverse().Multiply(vector)));
        }

        private void _setZoom(float zoom)
        {
            _zoom01 = BMath.Clamp(zoom, _config.minViewDistance / _maxViewDistance, 1f);
            _viewDistance = _maxViewDistance * _zoom01;

            _debugTrack();
        }

        private void _debugTrack()
        {
            BDebug.Track(this, nameof(_position), _position);
            BDebug.Track(this, nameof(_viewDistance), _viewDistance);
            BDebug.Track(this, nameof(_maxViewDistance), _maxViewDistance);
            BDebug.Track(this, nameof(_zoom01), _zoom01);
            BDebug.Track(this, "pitch", _rotationRad.pitch * BMath.Rad2Deg);
            BDebug.Track(this, "roll", _rotationRad.roll * BMath.Rad2Deg);
        }
    }
}