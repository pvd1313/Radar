using System.Diagnostics;
using System.Xml.Schema;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Serialization;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraTransform : Actor, ICameraTransform
    {
        public bool IsReady { get; private set; }
        public float2 Position => (float2)_position;
        public IsometricCameraRotation RotationRad => _rotationRad;
        public float Zoom01 => _zoom01;
        public float ViewDistance => _viewDistance;

        [Required] private readonly IScreen _screen;
        [Required] private readonly CameraTransformConfig _config;
        [Required] private readonly GameModel _game;
        [Required] private readonly IWorldManager _worldManager;

        private float2x2 _rotation;
        private float2x2 _scale;
        private float2x2 _rotationInverse;
        private float2x2 _scaleInverse;
        private float _yScaleRollCos;
        
        private IsometricCameraRotation _rotationRad;
        private float _viewDistance;
        private float _maxViewDistance;
        private float _zoom01;
        private float _zoomMax;
        private double2 _position;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _rotationRad = _config.isometricRotationDeg * BMath.DEG2_RAD;
            
            _rotation = float2x2.Rotate(_rotationRad.roll);
            _rotationInverse = float2x2.Rotate(_rotationRad.roll * -1);

            _scale = float2x2.Scale(1, BMath.Cos(_rotationRad.pitch));
            _scaleInverse = _scale.Inverse();

            _yScaleRollCos = BMath.Cos(_rotationRad.roll);

            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            _position = _game.camera.position;

            _zoomMax = _config.enablePlanetMode ? 0.25f : _config.plainModeViewDistance01;
            _maxViewDistance = _worldManager.GetCurrentWorld().terrain.terrainSize * _zoomMax;

            _setZoom(_game.camera.zoom01);
            
            IsReady = true;

            return UniTask.CompletedTask;
        }

        protected override void _gameSave(BlockingContext context)
        {
            _game.camera.position = (float2)_position;
            _game.camera.zoom01 = _zoom01;
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

        public void SetPosition(float2 position)
        {
            _position = position;
            
            _debugTrack();
        }

        public void AddZoom(float zoomDelta)
        {
            _setZoom(_zoom01 + zoomDelta);
        }
        
        public float2 ScreenToWorld(float2 screenPoint)
        {
            return ViewportToWorld(screenPoint / _screen.Size);
        }

        public float2 WorldToScreen(float2 worldPoint)
        {
            return WorldToViewport(worldPoint) * _screen.Size;
        }

        public float2 ViewportToWorld(float2 viewport)
        {
            float distance = _viewDistance * _yScaleRollCos;
            
            float2 offset = _localToWorld(new float2(_screen.Aspect, 1) * distance * (viewport - 0.5f));
            
            return (float2)_position + offset;
        }

        public Bounds2D GetViewBounds()
        {
            float2 pos0 = ViewportToWorld(new float2(0, 0));
            float2 pos1 = ViewportToWorld(new float2(0, 1));
            float2 pos2 = ViewportToWorld(new float2(1, 0));
            float2 pos3 = ViewportToWorld(new float2(1, 1));

            float xMin = BMath.Min(BMath.Min(pos0.x, pos1.x), BMath.Min(pos2.x, pos3.x));
            float xMax = BMath.Max(BMath.Max(pos0.x, pos1.x), BMath.Max(pos2.x, pos3.x));

            float yMin = BMath.Min(BMath.Min(pos0.y, pos1.y), BMath.Min(pos2.y, pos3.y));
            float yMax = BMath.Max(BMath.Max(pos0.y, pos1.y), BMath.Max(pos2.y, pos3.y));
            
            return new Bounds2D(xMin, xMax, yMin, yMax);
        }

        public float2 WorldToViewport(float2 worldPoint)
        {
            float2 localPoint = _worldToLocal(worldPoint - (float2)_position);

            localPoint.x /= _viewDistance * _yScaleRollCos * _screen.Aspect;
            localPoint.y /= _viewDistance * _yScaleRollCos;
            
            return localPoint + 0.5f;
        }

        private float2 _worldToLocal(float2 vector)
        {
            return _scale.Mul(_rotation.Mul(vector));
        }

        private float2 _localToWorld(float2 vector)
        {
            return _rotationInverse.Mul(_scaleInverse.Mul(vector));
        }

        private void _setZoom(float zoom)
        {
            _zoom01 = BMath.Clamp(zoom, _config.minViewDistance / _maxViewDistance, 1f);
            _viewDistance = _maxViewDistance * _zoom01;

            _debugTrack();
        }

        [Conditional("DEVELOPMENT")]
        private void _debugTrack()
        {
            BDebug.Track(this, nameof(_position), (float2)_position);
            BDebug.Track(this, nameof(_viewDistance), _viewDistance);
            BDebug.Track(this, nameof(_maxViewDistance), _maxViewDistance);
            BDebug.Track(this, nameof(_zoom01), _zoom01);
            BDebug.Track(this, nameof(_yScaleRollCos), _yScaleRollCos);
            BDebug.Track(this, "pitch", _rotationRad.pitch * BMath.RAD_2DEG);
            BDebug.Track(this, "roll", _rotationRad.roll * BMath.RAD_2DEG);
        }
    }
}