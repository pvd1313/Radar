using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public struct CameraControllerState
    {
        public float zoom01;
        public float2 position;

        public override string ToString()
        {
            return $"{{zoom01: {zoom01:N3}; position: ({position.x:N0}, {position.y:N0})}}";
        }
    }
}