using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public interface ICameraController
    {
        public float2 Position { get; }
        public float ViewDistance { get; }
    }
}