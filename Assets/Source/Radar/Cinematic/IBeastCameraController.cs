using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public interface IBeastCameraController
    {
        public float2 Position { get; }
        public float ViewDistance { get; }
    }
}