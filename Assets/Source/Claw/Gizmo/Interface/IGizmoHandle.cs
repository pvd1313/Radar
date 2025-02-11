using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmo
{
    public interface IGizmoHandle
    {
        void SetColor(Color color);
        
        void Clear();
        
        void AddPoint(float2 position);
        
        void AddPoint(float2 position, float size);
        
        void AddQuad(float2 position, float worldSize);

        void Dispose();
    }
}