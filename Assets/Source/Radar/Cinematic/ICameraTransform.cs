using Beast.Claw.Common;
using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public interface ICameraTransform : IReady
    {
        public IsometricCameraRotation MaxRotationRad { get; }
        public float2 Position { get; }
        public IsometricCameraRotation CurrentRotationRad { get; }
        public float Zoom01 { get; }
        float ViewDistance { get; }
        
        float2 ScreenPointToWorld(float2 screenPoint);
        float2 ViewportToWorld(float2 viewport);
        
        void AddPosition(float2 delta);
        
        void AddZoom(float zoomDelta);
    }
}