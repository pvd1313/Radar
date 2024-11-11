using Unity.Mathematics;

namespace Beast.Claw.Graphic
{
    public interface ICamera
    {
        public bool IsPointInsideUI(float2 viewportPoint);
    }
}