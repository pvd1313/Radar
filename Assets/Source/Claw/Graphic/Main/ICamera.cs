using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public interface ICamera
    {
        public Camera Camera { get; }
        
        public bool IsPointInsideUI(float2 viewportPoint);
    }
}