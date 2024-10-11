using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public interface ICameraTransform : IReady
    {
        public float2 Position { get; }
        public IsometricCameraRotation RotationRad { get; }
        public float Zoom01 { get; }
        float ViewDistance { get; }
        
        float2 ScreenPointToWorld(float2 screenPoint);
        float2 ViewportToWorld(float2 viewport);
        float2 WorldToViewport(float2 worldPoint);
        void AddPosition(float2 delta, Space space);
        void AddZoom(float zoomDelta);
    }
}