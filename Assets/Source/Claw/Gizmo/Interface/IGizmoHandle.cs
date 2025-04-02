using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmo
{
    public interface IGizmoHandle
    {
        void SetColor(Color color);
        
        void Clear();
        
        void AddScreenPoint(float2 position);
        
        void AddScreenPoint(float2 position, float size);
        
        void AddWorldQuad(float2 position, float worldSize);

        void Dispose();
    }
}