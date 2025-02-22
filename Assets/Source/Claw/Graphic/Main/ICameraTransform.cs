using Beast.Claw.Common;
using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public interface ICameraTransform : IReady
    {
        public float2 Position { get; }
        public IsometricCameraRotation RotationRad { get; }
        public float Zoom01 { get; }
        float ViewDistance { get; }
        
        float2 ScreenToWorld(float2 screenPoint);
        float2 ViewportToWorld(float2 viewport);
        Bounds2D GetViewBounds();
        float2 WorldToViewport(float2 worldPoint);
        public float2 WorldToScreen(float2 worldPoint);
        void AddPosition(float2 delta, Space space);
        void AddZoom(float zoomDelta);
        void SetPosition(float2 position);
    }
}