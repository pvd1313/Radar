using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmo
{
    public interface IGizmoHandle : IDisposable
    {
        void SetColor(Color color);
        
        void Clear();
        
        void AddPoint(float2 position);
        
        void AddPoint(float2 position, float size);

        void OverrideLastPoint(float2 position, float size);
        
        void AddQuad(float2 position, float worldSize);
    }
}